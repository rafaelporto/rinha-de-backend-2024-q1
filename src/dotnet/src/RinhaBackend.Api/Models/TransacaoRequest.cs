namespace RinhaBackend.Api.Models;

[GenerateSerializer, Alias(nameof(TransacaoRequest))]
public readonly struct TransacaoRequest(uint valor, char tipo, string descricao)
{
    [Id(0)]
    public uint Valor { get; } = valor;
    [Id(1)]
    public char Tipo { get; } = tipo;
    [Id(2)]
    public string Descricao { get; } = descricao;

    public readonly bool EhDebito => Tipo == 'd';
}
