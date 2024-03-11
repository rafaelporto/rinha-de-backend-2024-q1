using System.Text.Json;
using Microsoft.Extensions.ObjectPool;
using RinhaBackend.Api.Data;
using RinhaBackend.Api.Models;

namespace RinhaBackend.Api.Grains;

public sealed class ContaGrain : Grain, IContaGrain
{
    private Conta Conta { get; set; }
    private readonly IStore _store;
    private readonly ObjectPool<TransacaoEntidade> _transacaoPool;
    private bool _extratoDesatualizado = false;
    private byte[] _extratoSerializado = [];
    private readonly object _lockExtrato = new();

    public ContaGrain(IStore store, ObjectPool<TransacaoEntidade> transacaoPool)
    {
        _store = store;
        _transacaoPool = transacaoPool;
        Conta = new() { Id = (int)this.GetPrimaryKeyLong() };
    }

    public async ValueTask<GrainResponse> CreditarValor(TransacaoRequest transacaoRequest)
    {
        var (conta, transacao) = Conta.CreditarValor(transacaoRequest.Valor, transacaoRequest.Descricao!);
        Conta = conta;

        lock (_lockExtrato)
        {
            _extratoDesatualizado = true;
        }

        var dbEntity = _transacaoPool.Get();
        dbEntity.ContaId = Conta.Id;
        dbEntity.Transacao = transacao;

        await _store.Insert(dbEntity).ConfigureAwait(false);

        return GrainResponse.Ok(ContaSaldoSerializado());
    }

    public async ValueTask<GrainResponse> DebitarValor(TransacaoRequest transacaoRequest)
    {
        Result<(Conta Conta, Transacao Transacao)> result = Conta.DebitarValor(transacaoRequest.Valor, transacaoRequest.Descricao!);

        if (result.IsSuccess)
        {
            Conta = result.Value.Conta;
            var dbEntity = _transacaoPool.Get();
            dbEntity.ContaId = Conta.Id;
            dbEntity.Transacao = result.Value.Transacao;

            await _store.Insert(dbEntity).ConfigureAwait(false);

            lock (_lockExtrato)
            {
                _extratoDesatualizado = true;
            }

            return GrainResponse.Ok(ContaSaldoSerializado());
        }

        return GrainResponse.UnprocessableEntity(result.Error);
    }

    public ValueTask<GrainResponse> ObterExtrato()
    {

        lock (_lockExtrato)
        {
            if (_extratoDesatualizado)
                AtualizarExtratoSerializado();
        }


        return new ValueTask<GrainResponse>(GrainResponse.Ok(_extratoSerializado));
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        Conta = await _store.ReadContaAsync((int)this.GetPrimaryKeyLong());
        AtualizarExtratoSerializado();

        await base.OnActivateAsync(cancellationToken);
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

    private byte[] ContaSaldoSerializado() =>
        JsonSerializer.SerializeToUtf8Bytes(
            new ContaSaldo(Conta.Limite, Conta.Saldo),
            JsonContext.Default.ContaSaldo);

    private void AtualizarExtratoSerializado()
    {
        ContaExtrato extrato = new(new ContaSaldoExtrato(Conta.Limite, Conta.Saldo), [.. Conta.Extrato]);
        _extratoSerializado = JsonSerializer.SerializeToUtf8Bytes(extrato, JsonContext.Default.ContaExtrato);
        _extratoDesatualizado = false;
    }
}
