using RinhaBackend.Api.Endpoints;
using RinhaBackend.Api.Models;

namespace RinhaBackend.Api.Grains;

[Alias("IContaGrain")]
public interface IContaGrain : IGrainWithIntegerKey
{
    [Alias("CreditarValor")]
    ValueTask<GrainResponse> CreditarValor(TransacaoRequest transacao);

    [Alias("DebitarValor")]
    ValueTask<GrainResponse> DebitarValor(TransacaoRequest transacao);

    [Alias("ObterExtrato")]
    ValueTask<GrainResponse> ObterExtrato();
}
