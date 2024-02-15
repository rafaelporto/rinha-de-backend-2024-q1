using RinhaBackend.Api.Grains;

namespace RinhaBackend.Api.Endpoints
{
    public static class CriarContasEndpoint
    {
        public static RouteHandlerBuilder MapPostCriarContas(this IEndpointRouteBuilder app)
        {
            return app.MapPost("/contas",
                        async (IGrainFactory grainFactory) =>
            {
                await grainFactory.GetGrain<IContaGrain>(1).CriarConta(new(100000, 0));
                await grainFactory.GetGrain<IContaGrain>(2).CriarConta(new(80000, 0));
                await grainFactory.GetGrain<IContaGrain>(3).CriarConta(new(1000000, 0));
                await grainFactory.GetGrain<IContaGrain>(4).CriarConta(new(10000000, 0));
                await grainFactory.GetGrain<IContaGrain>(5).CriarConta(new(500000, 0));

                return Results.Created();
            });
        }
    }

    public record CriarContaRequest(int Id, string Nome);
}
