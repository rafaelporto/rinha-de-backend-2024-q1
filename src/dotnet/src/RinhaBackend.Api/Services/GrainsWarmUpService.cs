using RinhaBackend.Api.Grains;

namespace RinhaBackend.Api.Services;

public sealed class GrainsWarmUpService : IHostedService
{
    private readonly IGrainFactory _grainFactory;

    public GrainsWarmUpService(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var firstGrain = _grainFactory.GetGrain<IContaGrain>("1");
        var secondGrain = _grainFactory.GetGrain<IContaGrain>("2");
        var thirdGrain = _grainFactory.GetGrain<IContaGrain>("3");
        var fourthGrain = _grainFactory.GetGrain<IContaGrain>("4");
        var fifthGrain = _grainFactory.GetGrain<IContaGrain>("5");

        await Task.WhenAll(
            firstGrain.CarregarOuCriarConta(new(100000, 0)),
            secondGrain.CarregarOuCriarConta(new(80000, 0)),
            thirdGrain.CarregarOuCriarConta(new(1000000, 0)),
            fourthGrain.CarregarOuCriarConta(new(10000000, 0)),
            fifthGrain.CarregarOuCriarConta(new(500000, 0))
        );

        Console.WriteLine("[GrainsWarmUpService] => Grãos ativados.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("GrainsWarmUpService is stopping.");

        return Task.CompletedTask;
    }
}
