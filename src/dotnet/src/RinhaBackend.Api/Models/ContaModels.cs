using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace RinhaBackend.Api.Models;

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
    public DateTimeOffset DataExtrato { get; set; }

    public ContaSaldoExtrato(uint limite, int total)
    {
        Limite = limite;
        Total = total;
        DataExtrato = DateTimeOffset.UtcNow;
    }
}

[GenerateSerializer, Alias(nameof(ContaExtrato))]
public record struct ContaExtrato
{
    [Id(0)]
    public ContaSaldoExtrato Saldo { get; set; }

    [Id(1)]
    [JsonPropertyName("ultimas_transacoes")]
    public ImmutableArray<Transacao> UltimasTransacoes { get; set; }

    public ContaExtrato(ContaSaldoExtrato saldo, ImmutableArray<Transacao> extrato)
    {
        Saldo = saldo;
        UltimasTransacoes = extrato;
    }
}
