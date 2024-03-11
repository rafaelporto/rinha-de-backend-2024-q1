using Microsoft.Extensions.ObjectPool;
using RinhaBackend.Api.Models;

namespace RinhaBackend.Api.Data;

public sealed class TransacaoEntidade: IResettable
{
    public int? Id { get; set; } = default!;
    public int ContaId { get; set; } = default!;
    public Transacao Transacao { get; set; } = default!;

    public TransacaoEntidade()
    { }

    private TransacaoEntidade(int? id, int contaId, Transacao transacao) =>
        (Id, ContaId, Transacao) = (id, contaId, transacao);
    
    public static TransacaoEntidade New(int contaId, Transacao transacao) =>
        new(null, contaId, transacao);

    public bool TryReset()
    {
        Id = default!;
        ContaId = default!;
        Transacao = default!;
        return true;
    }
}
