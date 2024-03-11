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

    public (Conta conta, Transacao transacao) CreditarValor(uint valor, string descricao)
    {
        Saldo += (int)valor;
        var transacao = AdicionarEntradaExtrato(descricao, valor, 'c');
        return (this, transacao);
    }

    public Result<(Conta conta, Transacao transacao)> DebitarValor(uint valor, string descricao)
    {
        if (Saldo + Limite - valor < 0)
            return Result.Failure<(Conta, Transacao)>();

        Saldo -= (int)valor;
        var transacao = AdicionarEntradaExtrato(descricao, valor, 'd');
        return Result.Success((this, transacao));
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
