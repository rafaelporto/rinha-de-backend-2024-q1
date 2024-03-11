using System.Buffers;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using RinhaBackend.Api.Models;

namespace RinhaBackend.Api.Data;

public interface IStore
{
    ValueTask Insert(TransacaoEntidade entity);
    ValueTask<Conta> ReadContaAsync(int id);
    ValueTask UpsertAsync(GrainEntity<Conta> entity);
    int TransacoesEmProcessamento { get; }
}

public sealed partial class Store : BackgroundService, IStore, IDisposable
{
    private readonly Channel<TransacaoEntidade> _transacaoChannel;
    private readonly ChannelWriter<TransacaoEntidade> _channelWriter;
    private readonly ChannelReader<TransacaoEntidade> _channelReader;
    private readonly ClusterConfig _options;
    const int MAX_BULK_INSERT = 1_000;
    public int TransacoesEmProcessamento => _channelReader.Count;
    private readonly StringBuilder _insertBuilder = new();
    private readonly ObjectPool<TransacaoEntidade> _transacaoPool;

    public Store(IOptions<ClusterConfig> options, ObjectPool<TransacaoEntidade> transacaoPool)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Value);
        _options = options.Value;
        _transacaoPool = transacaoPool;

        _transacaoChannel = Channel.CreateBounded<TransacaoEntidade>(
        new BoundedChannelOptions(MAX_BULK_INSERT)
        {
            SingleWriter = false,
            SingleReader = true,
            AllowSynchronousContinuations = false,
            FullMode = BoundedChannelFullMode.Wait
        });
        _channelWriter = _transacaoChannel.Writer;
        _channelReader = _transacaoChannel.Reader;
    }

    public ValueTask Insert(TransacaoEntidade entity) =>
        _channelWriter.WriteAsync(entity);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            TransacaoEntidade[] dbEntryPool = ArrayPool<TransacaoEntidade>.Shared.Rent(MAX_BULK_INSERT);

            await _channelReader.WaitToReadAsync(stoppingToken);

            int index = 0;
            while (index < MAX_BULK_INSERT && _channelReader.TryRead(out var entry))
                dbEntryPool[index++] = entry;

            if (index == 0)
                continue;

            using var dbConnection = new MySqlConnection(_options.ConnectionString);
            try
            {
                using var command = dbConnection.CreateCommand();

                _insertBuilder.Append("INSERT INTO transacoes (conta_id, content) VALUES ");
                
                for (int i = 0; i < index; i++)
                {
                    _insertBuilder.Append("(@conta_id").Append(i)
                                  .Append(", ")
                                  .Append("@content").Append(i)
                                  .Append(')');

                    if (i + 1 < index)
                        _insertBuilder.Append(',');
                    else
                        _insertBuilder.AppendLine();
                    
                    var entity = dbEntryPool[i];
                    command.Parameters.AddWithValue($"@conta_id{i}", entity.ContaId);
                    command.Parameters.AddWithValue($"@content{i}", 
                        JsonSerializer.SerializeToUtf8Bytes(entity.Transacao, JsonContext.Default.Transacao));
                }

                command.CommandText = _insertBuilder.ToString();
                command.CommandType = CommandType.Text;
            
                await dbConnection.OpenAsync(stoppingToken);
                await command.ExecuteNonQueryAsync(stoppingToken);

            }
            finally
            {
                await dbConnection.CloseAsync();
                _insertBuilder.Clear();

                ArrayPool<TransacaoEntidade>.Shared.Return(dbEntryPool);
                
                for (int i = 0; i < index; i++)
                    _transacaoPool.Return(dbEntryPool[i]);
            }
        }

    }

    public async ValueTask<Conta> ReadContaAsync(int id)
    {
        using var dbConnection = new MySqlConnection(_options.ConnectionString);

        try
        {
            await dbConnection.OpenAsync();

            using var command = dbConnection.CreateCommand();
            command.CommandText = "SELECT * FROM contas WHERE id = @id";
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();

            if (reader.Read())
            {
                var content = reader["content"].ToString()!;
                return JsonSerializer.Deserialize(content, JsonContext.Default.Conta);
            }
        }
        finally
        {
            await dbConnection.CloseAsync();
        }

        throw new InvalidOperationException("Conta not found");
    }

    public async ValueTask UpsertAsync(GrainEntity<Conta> entity)
    {
        using var dbConnection = new MySqlConnection(_options.ConnectionString);

        try
        {
            await dbConnection.OpenAsync();

            var sql = """
                INSERT INTO contas (id, content)
                VALUES (@id, @content) 
                ON DUPLICATE KEY UPDATE 
                content = VALUES(content)
            """;

            using var command = new MySqlCommand(sql, dbConnection);

            command.Parameters.AddWithValue("@id", entity.Id);
            command.Parameters.AddWithValue("@content", 
                JsonSerializer.SerializeToUtf8Bytes(entity.State, JsonContext.Default.Conta));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            await dbConnection.CloseAsync();
        }
    }

    public override void Dispose()
    {
        _channelReader.Completion.Wait();
        base.Dispose();
    }
}
