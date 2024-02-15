using RinhaBackend.Api.Models;

namespace RinhaBackend.Api.Grains;

[Alias("IContaGrain")]
public interface IContaGrain : IGrainWithIntegerKey
{
    [Alias("CriarConta")]
    ValueTask CriarConta(NovaConta conta);

    [Alias("CreditarValor")]
    ValueTask<ContaSaldo> CreditarValor(uint valor, string descricao);

    [Alias("DebitarValor")]
    ValueTask<ContaSaldo> DebitarValor(uint valor, string descricao);

    [Alias("ObterExtrato")]
    ValueTask<ContaExtrato> ObterExtrato();
}
