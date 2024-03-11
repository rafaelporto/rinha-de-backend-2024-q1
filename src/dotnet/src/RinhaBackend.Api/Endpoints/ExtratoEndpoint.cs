using RinhaBackend.Api.Grains;

namespace RinhaBackend.Api.Endpoints;

public static class ExtratoEndpoint
{
    public static RouteHandlerBuilder MapGetExtrato(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/clientes/{id:regex([1-5])}/extrato",
            async (int id, IGrainFactory grains, HttpResponse response) =>
        {
            var conta = grains.GetGrain<IContaGrain>(id);
            GrainResponse grainResponse = await conta.ObterExtrato();

            response.ContentType = "application/json";
            response.StatusCode = grainResponse.Code;
            
            if (grainResponse.IsSuccess)
                await response.Body.WriteAsync(grainResponse.Body);
        });
    }
}
