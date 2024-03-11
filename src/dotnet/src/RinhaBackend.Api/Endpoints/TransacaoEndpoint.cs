using System.Text.Json;
using RinhaBackend.Api.Grains;
using RinhaBackend.Api.Models;

namespace RinhaBackend.Api.Endpoints;

public static class TransacaoEndpoint
{
    public static RouteHandlerBuilder MapPostTransacao(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/clientes/{id:regex([1-5])}/transacoes",
                    async (int id,
                        IGrainFactory grains,
                        Stream requestBody, HttpResponse response) =>
        {
            var rawBody = await new StreamReader(requestBody).ReadToEndAsync();

            response.ContentType = "application/json";

            if (string.IsNullOrWhiteSpace(rawBody))
            {
                response.StatusCode = 422;
                return;
            }

            JsonDocument body = JsonDocument.Parse(rawBody);

            var transacaoResult = IsValid(body);

            if (transacaoResult.IsFailure)
            {
                response.StatusCode = 422;
                return;
            }

            var transacao = transacaoResult.Value;
            var conta = grains.GetGrain<IContaGrain>(id);

            GrainResponse result = transacao.EhDebito ?
                await conta.DebitarValor(transacao) :
                await conta.CreditarValor(transacao);

            response.StatusCode = result.Code;
            
            if (result.IsSuccess)
                await response.Body.WriteAsync(result.Body);
        });
    }

    private static Result<TransacaoRequest> IsValid(JsonDocument body)
    {
        if (body.RootElement.GetProperty("valor").TryGetDouble(out var valor) is false)
            return Result.Failure<TransacaoRequest>();

        if (valor == 0 || (valor % 1) != 0)
            return Result.Failure<TransacaoRequest>();

        var tipoStr = body.RootElement.GetProperty("tipo").GetString();

        if (string.IsNullOrWhiteSpace(tipoStr))
            return Result.Failure<TransacaoRequest>();

        if (tipoStr != "d" && tipoStr != "c")
            return Result.Failure<TransacaoRequest>();

        var descricao = body.RootElement.GetProperty("descricao").GetString();

        if (string.IsNullOrWhiteSpace(descricao) || descricao.Length > 10)
            return Result.Failure<TransacaoRequest>();

        char tipo = tipoStr == "d" ? 'd' : 'c';

        TransacaoRequest transacao = new(Convert.ToUInt32(valor), tipo, descricao);

        return Result.Success(transacao);
    }
}
