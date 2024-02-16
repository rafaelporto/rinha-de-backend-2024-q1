using RinhaBackend.Api.Models;

namespace RinhaBackend.Api.Grains;

public sealed class ContaGrain(
    [PersistentState(
        stateName: "conta",
        storageName: "contas")]
        IPersistentState<Conta> state)
    : Grain, IContaGrain
{

    public async ValueTask CriarConta(NovaConta conta)
    {
        state.State = new()
        {
            Id = (int)this.GetPrimaryKeyLong(),
            Limite = conta.Limite,
            Saldo = conta.Saldo
        };

        await state.WriteStateAsync();
    }

    public ValueTask<ContaSaldo> CreditarValor(uint valor, string descricao)
    {
        state.State = state.State.CreditarValor(valor, descricao);

        return new ValueTask<ContaSaldo>(
                new ContaSaldo(state.State.Limite, state.State.Saldo));
    }


    public ValueTask<(bool valido, ContaSaldo saldo)> DebitarValor(uint valor, string descricao)
    {
        var (valido, saldo) = state.State.DebitarValor(valor, descricao);

        if (valido)
            state.State = saldo;

        return new ValueTask<(bool, ContaSaldo)>(
                (valido,
                new ContaSaldo(state.State.Limite, state.State.Saldo)));
    }

    public ValueTask<ContaExtrato> ObterExtrato()
    {
        Conta conta = state.State;
        ContaExtrato extrato = new(new ContaSaldoExtrato(conta.Limite, conta.Saldo), conta.Extrato);

        return new ValueTask<ContaExtrato>(extrato);
    }
}

