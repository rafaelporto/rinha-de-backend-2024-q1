using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using RinhaBackend.Api.Grains;
using RinhaBackend.Api.Models;

namespace RinhaBackend.Api.Endpoints;

public static class TransacaoEndpoint
{
    public static RouteHandlerBuilder MapPostTransacao(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/clientes/{id:regex([1-5])}/transacoes",
                    async (int id,
                     [FromBody] Transacao transacao,
                     IGrainFactory grains) =>
        {
            if (transacao.IsInvalid())
                return Results.UnprocessableEntity();

            if (int.TryParse(transacao.Valor.ToString(), out int valor) is false)
                return Results.UnprocessableEntity();

            if (valor <= 0)
                return Results.UnprocessableEntity();

            var conta = grains.GetGrain<IContaGrain>(id);

            if (transacao.IsDebito)
            {
                (bool valido, ContaSaldo saldo) = await conta.DebitarValor((uint)valor,
                                                transacao.Descricao!);

                return valido
                    ? Results.Ok(saldo)
                    : Results.UnprocessableEntity();
            }

            return Results.Ok(await conta.CreditarValor((uint)valor, transacao.Descricao!));
        });
    }
}

public record Transacao(object Valor, char? Tipo, string? Descricao)
{
    [JsonIgnore]
    public bool IsDebito => Tipo == 'd';

    public bool IsValid()
    {
        return Valor is not null
        && Tipo.HasValue
        && string.IsNullOrWhiteSpace(Descricao) is false
        && Descricao.Length <= 10
        && (Tipo == 'd' || Tipo == 'c');
    }

    public bool IsInvalid() => !IsValid();
}
