using System.Text.Json.Serialization;

namespace RinhaBackend.Api.Models;

[GenerateSerializer, Alias(nameof(Transacao))]
public class Transacao
{
    [Id(0)]
    public string Descricao { get; set; }

    [Id(1)]
    public uint Valor { get; set; }

    [Id(2)]
    public char Tipo { get; set; }

    [Id(3)]
    [JsonPropertyName("realizada_em")]
    public DateTimeOffset RealizadaEm { get; set; }

    public Transacao()
    {
        Descricao = string.Empty;
    }

    public Transacao(string descricao, uint valor, char tipo, DateTimeOffset realizadoEm) : base()
    {
        Descricao = descricao;
        Valor = valor;
        Tipo = tipo;
        RealizadaEm = realizadoEm;
    }
}
