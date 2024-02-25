using RinhaBackend.Api.Models;

namespace RinhaBackend.Api.Data;

public sealed class TransacaoEntity
{
    public string? Id { get; set; } = default!;
    public string ContaId { get; set; } = default!;
    public Transacao Transacao { get; set; }

    private TransacaoEntity(string? id, string contaId, Transacao transacao) =>
        (Id, ContaId, Transacao) = (id, contaId, transacao);
    
    public static TransacaoEntity New(string contaId, Transacao transacao) =>
        new(null, contaId, transacao);
}
