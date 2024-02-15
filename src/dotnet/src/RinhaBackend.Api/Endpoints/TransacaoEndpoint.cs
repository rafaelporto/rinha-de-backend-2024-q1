using Microsoft.AspNetCore.Mvc;
using RinhaBackend.Api.Grains;

namespace RinhaBackend.Api.Endpoints;

public static class TransacaoEndpoint
{
    public static RouteHandlerBuilder MapPostTransacao(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/clientes/{id:int}/transacoes",
                    async (int id,
                     [FromBody] TransacaoRequest transacao,
                     IGrainFactory grains) =>
        {
            var conta = grains.GetGrain<IContaGrain>(id);

            if (transacao.Tipo == 'd')
                return Results.Ok(await conta.DebitarValor(transacao.Valor, transacao.Descricao));

            return Results.Ok(await conta.CreditarValor(transacao.Valor, transacao.Descricao));
        });
    }
}

public record TransacaoRequest(uint Valor, char Tipo, string Descricao);
