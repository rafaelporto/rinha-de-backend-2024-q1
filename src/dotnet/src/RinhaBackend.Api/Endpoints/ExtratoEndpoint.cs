using RinhaBackend.Api.Grains;

namespace RinhaBackend.Api.Endpoints;

public static class ExtratoEndpoint
{
    public static RouteHandlerBuilder MapGetExtrato(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/clientes/{id:regex([1-5])}/extrato", async (int id, IGrainFactory grains) =>
        {
            var conta = grains.GetGrain<IContaGrain>(id);
            var extrato = await conta.ObterExtrato();
            return Results.Ok(extrato);
        })
        .WithName("GetExtrato")
        .WithOpenApi();
    }
}
