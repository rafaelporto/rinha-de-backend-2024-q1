using System.Buffers;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using RinhaBackend.Api.Models;

namespace RinhaBackend.Api.Data;

public interface IStore
{
    ValueTask Insert(TransacaoEntity entity);
    ValueTask<Conta?> ReadContaAsync(int id);
    ValueTask UpsertAsync(GrainEntity<Conta> entity);
}

public sealed partial class Store : BackgroundService, IStore, IDisposable
{
    private readonly Channel<TransacaoEntity> _transacaoChannel;
    private readonly ChannelWriter<TransacaoEntity> _channelWriter;
    private readonly ChannelReader<TransacaoEntity> _channelReader;
    private readonly ClusterConfig _options;
    const int MAX_BULK_INSERT = 1_000;

    public Store(IOptions<ClusterConfig> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;

        _transacaoChannel = Channel.CreateBounded<TransacaoEntity>(1000);
        _channelWriter = _transacaoChannel.Writer;
        _channelReader = _transacaoChannel.Reader;
    }

    public async ValueTask Insert(TransacaoEntity entity) =>
        await _channelWriter.WriteAsync(entity);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TransacaoEntity[] dbEntryPool = ArrayPool<TransacaoEntity>.Shared.Rent(MAX_BULK_INSERT);

        while (!stoppingToken.IsCancellationRequested)
        {
            await _channelReader.WaitToReadAsync(stoppingToken);

            int index = 0;
            while (index < MAX_BULK_INSERT && _channelReader.TryRead(out var entry))
            {
                dbEntryPool[index++] = entry;
            }

            if (index == 0)
                continue;

            using var dbConnection = new MySqlConnection(_options.ConnectionString);
            try
            {
                await dbConnection.OpenAsync(stoppingToken);

                using var command = dbConnection.CreateCommand();

                command.CommandText = "INSERT INTO transacoes (conta_id, content) VALUES (@conta_id, @content)";

                for (int i = 0; i < index; i++)
                {
                    var entity = dbEntryPool[i];
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@conta_id", entity.ContaId);
                    command.Parameters.AddWithValue("@content", JsonSerializer.SerializeToUtf8Bytes(entity.Transacao));
                }

                await command.ExecuteNonQueryAsync(stoppingToken);

            }
            finally
            {
                await dbConnection.CloseAsync();
            }
        }

        ArrayPool<TransacaoEntity>.Shared.Return(dbEntryPool);
    }



    public override void Dispose()
    {
        _channelReader.Completion.Wait();
        base.Dispose();
    }

    public async ValueTask<Conta?> ReadContaAsync(int id)
    {
        using var dbConnection = new MySqlConnection(_options.ConnectionString);

        try
        {
            dbConnection.Open();

            using var command = dbConnection.CreateCommand();
            command.CommandText = "SELECT * FROM contas WHERE id = @id";
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();

            if (reader.Read())
            {
                var content = reader["content"].ToString()!;
                return JsonSerializer.Deserialize<Conta>(content);
            }
        }catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            dbConnection.Close();
        }

        return null;
    }

    public async ValueTask UpsertAsync(GrainEntity<Conta> entity)
    {
        using var dbConnection = new MySqlConnection(_options.ConnectionString);

        try
        {
            dbConnection.Open();

            var sql = """
                INSERT INTO contas (id, content)
                VALUES (@id, @content) 
                ON DUPLICATE KEY UPDATE 
                content = VALUES(content)
            """;

            using var command = new MySqlCommand(sql, dbConnection);

            command.Parameters.AddWithValue("@id", entity.Id);
            command.Parameters.AddWithValue("@content", JsonSerializer.SerializeToUtf8Bytes(entity.State));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            await dbConnection.CloseAsync();
        }
    }
}
