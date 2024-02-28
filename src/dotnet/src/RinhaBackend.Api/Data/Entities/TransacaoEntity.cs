using RinhaBackend.Api.Models;

namespace RinhaBackend.Api.Data;

public sealed class TransacaoEntity
{
    public int? Id { get; set; } = default!;
    public int ContaId { get; set; } = default!;
    public Transacao Transacao { get; set; }

    private TransacaoEntity(int? id, int contaId, Transacao transacao) =>
        (Id, ContaId, Transacao) = (id, contaId, transacao);
    
    public static TransacaoEntity New(int contaId, Transacao transacao) =>
        new(null, contaId, transacao);
}
