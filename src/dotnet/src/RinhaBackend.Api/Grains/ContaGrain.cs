using System.Text.Json;
using RinhaBackend.Api.Data;
using RinhaBackend.Api.Models;

namespace RinhaBackend.Api.Grains;

public sealed class ContaGrain : Grain, IContaGrain
{
    private Conta Conta { get; set; }
    private readonly IStore _store;

    public ContaGrain(IStore store)
    {
        _store = store;
        Conta = new() { Id = (int)this.GetPrimaryKeyLong() };
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var maybeConta = await _store.ReadContaAsync((int)this.GetPrimaryKeyLong());

        Conta = maybeConta ?? new() { Id = (int)this.GetPrimaryKeyLong() };

        await base.OnActivateAsync(cancellationToken);
    }

    public async ValueTask<ContaSaldo> CreditarValor(uint valor, string descricao)
    {
        Conta = Conta.CreditarValor(valor, descricao, out Transacao transacao);

        var dbEntity = TransacaoEntity.New(Conta.Id, transacao);

        await _store.Insert(dbEntity).ConfigureAwait(false);

        return new ContaSaldo(Conta.Limite, Conta.Saldo);
    }

    public async ValueTask<(bool valido, ContaSaldo saldo)> DebitarValor(uint valor, string descricao)
    {
        var (valido, saldo) = Conta.DebitarValor(valor, descricao, out var maybeTransacao);

        if (valido)
        {
            Conta = saldo;
            var transacaoEntity = TransacaoEntity.New(Conta.Id, maybeTransacao!);
            await _store.Insert(transacaoEntity).ConfigureAwait(false);
        }

        return (valido, new ContaSaldo(Conta.Limite, Conta.Saldo));
    }

    public ValueTask<ContaExtrato> ObterExtrato()
    {
        Conta conta = Conta;
        ContaExtrato extrato = new(new ContaSaldoExtrato(conta.Limite, conta.Saldo), [.. conta.Extrato]);

        return new ValueTask<ContaExtrato>(extrato);
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        await WriteStateAsync();
        await base.OnDeactivateAsync(reason, cancellationToken);
    }

    private async ValueTask WriteStateAsync()
    {
        GrainEntity<Conta> grainEntity = new(Conta.Id, Conta);
        await _store.UpsertAsync(grainEntity).ConfigureAwait(false);
    }
}
