using System.Buffers;
using System.Threading.Channels;
using Orleans;
using Raven.Client.Documents;
using Raven.Client.Documents.BulkInsert;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Session;
using Raven.Client.Exceptions.Database;
using Raven.Client.Http;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace RinhaBackend.Api.Data;

public interface IStore
{
    IAsyncDocumentSession OpenSession();
    ValueTask Insert(TransacaoEntity entity);
}

public sealed partial class Store: BackgroundService, IStore, IDisposable
{
    private const string DB_NAME = "rinhadb";
    private readonly DocumentStore _store;
    private readonly Channel<TransacaoEntity> _transacaoChannel;
    private readonly ChannelWriter<TransacaoEntity> _channelWriter;
    private readonly ChannelReader<TransacaoEntity> _channelReader;
    const int MAX_BULK_INSERT = 10_000;

    public Store()
    {
        _store = new DocumentStore()
        {
            Urls = ["http://localserver:8080"],
            Database = DB_NAME,
            Conventions = {
                DisableTcpCompression = false,
                HttpCompressionAlgorithm = HttpCompressionAlgorithm.Gzip,
            }
        };

        _store.Initialize();
        EnsureDatabaseIsCreated();

        _transacaoChannel = Channel.CreateBounded<TransacaoEntity>(1000);
        _channelWriter = _transacaoChannel.Writer;
        _channelReader = _transacaoChannel.Reader;
    }

    public void EnsureDatabaseIsCreated()
    {
        try
        {
            _store.Maintenance
                .ForDatabase(DB_NAME)
                .Send(new GetStatisticsOperation());
        }
        catch (DatabaseDoesNotExistException)
        {
            _store.Maintenance.Server
                .Send(new CreateDatabaseOperation(
                            new DatabaseRecord(DB_NAME)));
        }
    }

    public IAsyncDocumentSession OpenSession() => _store.OpenAsyncSession();

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

            BulkInsertOperation bulkInsert = default!;
            try
            {
                bulkInsert = _store.BulkInsert(token: stoppingToken);
                for (int i = 0; i < index; i++)
                {
                    await bulkInsert.StoreAsync(dbEntryPool[i]);
                }
            }
            finally
            {
                if (bulkInsert is not null)
                    await bulkInsert.DisposeAsync().ConfigureAwait(false);
            }
        }

        ArrayPool<TransacaoEntity>.Shared.Return(dbEntryPool);
    }

    public override void Dispose()
    {
        _store.Dispose();
        base.Dispose();
    }
}
