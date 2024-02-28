namespace RinhaBackend.Api.Models;

[GenerateSerializer, Alias(nameof(Conta))]
public sealed class Conta
{
    [Id(0)]
    public int Id { get; set; }

    [Id(1)]
    public uint Limite { get; set; } = 0;

    [Id(2)]
    public int Saldo { get; set; } = 0;

    [Id(3)]
    public LinkedList<Transacao> Extrato { get; set; } = new();

    public bool IsValid()
    {
        return Id > 0 && Limite > 0;
    }

    public Conta CreditarValor(uint valor, string descricao, out Transacao transacao)
    {
        Saldo += (int)valor;
        transacao = AdicionarEntradaExtrato(descricao, valor, 'c');
        return this;
    }

    public (bool, Conta) DebitarValor(uint valor, string descricao, out Transacao? transacao)
    {
        transacao = null;
        if (Saldo + Limite - valor < 0)
            return (false, this);

        Saldo -= (int)valor;
        transacao = AdicionarEntradaExtrato(descricao, valor, 'd');
        return (true, this);
    }

    public ContaSaldo ObterSaldo()
    {
        return new(Limite, Saldo);
    }

    private Transacao AdicionarEntradaExtrato(string descricao, in uint valor, in char tipo)
    {
        if (Extrato.Count == 10)
        {
            Extrato.RemoveLast();
        }

        var transacao = new Transacao(descricao, valor, tipo, DateTimeOffset.Now);
        Extrato.AddFirst(transacao);

        return transacao;
    }
}
