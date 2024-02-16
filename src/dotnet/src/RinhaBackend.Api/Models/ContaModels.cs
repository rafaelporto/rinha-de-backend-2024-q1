using System.Text.Json.Serialization;

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
    public Stack<EntradaExtrato> Extrato { get; set; } = new(10);

    public Conta CreditarValor(uint valor, string descricao)
    {
        Saldo += (int)valor;
        AdicionarEntradaExtrato(descricao, valor, 'c');
        return this;
    }

    public (bool, Conta) DebitarValor(uint valor, string descricao)
    {
        if (Saldo + Limite - valor < 0)
            return (false, this);

        Saldo -= (int)valor;
        AdicionarEntradaExtrato(descricao, valor, 'd');
        return (true, this);
    }

    public ContaSaldo ObterSaldo()
    {
        return new(Limite, Saldo);
    }

    private void AdicionarEntradaExtrato(string descricao, in uint valor, in char tipo)
    {
        if (Extrato.Count == 10)
        {
            EntradaExtrato _ = Extrato.Pop();
        }

        Extrato.Push(new EntradaExtrato(descricao, valor, tipo, DateTime.Now));
    }
}

[GenerateSerializer, Alias(nameof(NovaConta))]
public record struct NovaConta
{
    [Id(0)]
    public uint Limite { get; set; }
    [Id(1)]
    public int Saldo { get; set; }


    public NovaConta(uint limite, int saldo)
    {
        Limite = limite;
        Saldo = saldo;
    }
}

[GenerateSerializer, Alias(nameof(ContaSaldo))]
public record struct ContaSaldo
{
    [Id(0)]
    public uint Limite { get; set; }
    [Id(1)]
    public int Saldo { get; set; }

    public ContaSaldo(uint limite, int saldo)
    {
        Limite = limite;
        Saldo = saldo;
    }
}


[GenerateSerializer, Alias(nameof(ContaSaldoExtrato))]
public record struct ContaSaldoExtrato
{
    [Id(0)]
    public uint Limite { get; set; }
    [Id(1)]
    public int Total { get; set; }
    [Id(2)]
    [JsonPropertyName("data_extrato")]
    public DateTime DataExtrato { get; set; }

    public ContaSaldoExtrato(uint limite, int total)
    {
        Limite = limite;
        Total = total;
        DataExtrato = DateTime.UtcNow;
    }
}

[GenerateSerializer, Alias(nameof(EntradaExtrato))]
public record struct EntradaExtrato
{
    [Id(0)]
    public string Descricao { get; set; }

    [Id(1)]
    public uint Valor { get; set; }

    [Id(2)]
    public char Tipo { get; set; }

    [Id(3)]
    [JsonPropertyName("realizada_em")]
    public DateTime RealizadaEm { get; set; }

    public EntradaExtrato(string descricao, uint valor, char tipo, DateTime realizadoEm)
    {
        Descricao = descricao;
        Valor = valor;
        Tipo = tipo;
        RealizadaEm = realizadoEm;
    }
}

[GenerateSerializer, Alias(nameof(ContaExtrato))]
public record struct ContaExtrato
{
    [Id(0)]
    public ContaSaldoExtrato Saldo { get; set; }

    [Id(1)]
    [JsonPropertyName("ultimas_transacoes")]
    public Stack<EntradaExtrato> UltimasTransacoes { get; set; }

    public ContaExtrato(ContaSaldoExtrato saldo, Stack<EntradaExtrato> extrato)
    {
        Saldo = saldo;
        UltimasTransacoes = extrato;
    }
}

