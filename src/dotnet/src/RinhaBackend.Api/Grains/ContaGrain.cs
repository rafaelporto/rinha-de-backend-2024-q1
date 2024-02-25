using Orleans.Providers;
using RinhaBackend.Api.Data;
using RinhaBackend.Api.Models;

namespace RinhaBackend.Api.Grains;

[StorageProvider(ProviderName = "contasStorage")]
public sealed class ContaGrain(
    IStore store)
    : Grain<Conta>, IContaGrain
{
   public async ValueTask<ContaSaldo> CreditarValor(uint valor, string descricao)
    {
        State = State.CreditarValor(valor, descricao, out var transacao);

        var transacaoDbEntity = TransacaoEntity.New(State.Id, transacao);

        await store.Insert(transacaoDbEntity).ConfigureAwait(false);

        return new ContaSaldo(State.Limite, State.Saldo);
    }

    public async ValueTask<(bool valido, ContaSaldo saldo)> DebitarValor(uint valor, string descricao)
    {
        var (valido, saldo) = State.DebitarValor(valor, descricao, out var maybeTransacao);

        if (valido)
        {
            State = saldo;
            var transacaoEntity = TransacaoEntity.New(State.Id, maybeTransacao!);
            await store.Insert(transacaoEntity).ConfigureAwait(false);
        }

        return (valido, new ContaSaldo(State.Limite, State.Saldo));
    }

    public ValueTask<ContaExtrato> ObterExtrato()
    {
        Conta conta = State;
        ContaExtrato extrato = new(new ContaSaldoExtrato(conta.Limite, conta.Saldo), [.. conta.Extrato]);

        return new ValueTask<ContaExtrato>(extrato);
    }

    public async Task CarregarOuCriarConta(NovaConta novaConta)
    {
        if (State is not null)
            return;

        State = new()
        {
            Id = this.GetPrimaryKeyString(),
            Limite = novaConta.Limite,
            Saldo = novaConta.Saldo
        };

        await WriteStateAsync();
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        await WriteStateAsync();
        await base.OnDeactivateAsync(reason, cancellationToken);
    }
}
