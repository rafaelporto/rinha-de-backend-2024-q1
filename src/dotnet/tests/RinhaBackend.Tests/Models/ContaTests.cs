using MySqlX.XDevAPI.Common;
using RinhaBackend.Api;
using RinhaBackend.Api.Models;

namespace RinhaBackend.Tests.Contas;

public class ContaTests
{

    [Fact]
    public void Deve_creditar_valor_na_conta()
    {
        Conta conta = new()
        {
            Id = 1,
            Saldo = 100,
            Limite = 100000
        };
        (Conta conta, Transacao transacao) sut = conta.CreditarValor(100, "teste");
        Assert.Equal(200, sut.conta.Saldo);
    }

    [Fact]
    public void Deve_debitar_valor_na_conta()
    {
        Conta conta = new()
        {
            Id = 1,
            Saldo = 100,
            Limite = 100000
        };
        Result<(Conta conta, Transacao transacao)> sut = conta.DebitarValor(100, "teste");
        Assert.True(sut.IsSuccess);
        Assert.Equal(0, sut.Value.conta.Saldo);
    }

    [Fact]
    public void Deve_ser_invalido_debitar_quando_valor_maior_limite()
    {
        Conta conta = new()
        {
            Id = 1,
            Saldo = 0,
            Limite = 1000
        };

        Result<(Conta conta, Transacao transacao)> sut = conta.DebitarValor(1001, "teste");

        Assert.True(sut.IsFailure);
        Assert.Equal(default, sut.Value);
    }

//    [Fact]
//    public void Deve_adicionar_entrada_extrato_quando_creditar_valor()
//    {
//        Conta conta = new()
//        {
//            Id = 1,
//            Saldo = 100,
//            Limite = 100000
//        };
//
//        Conta sut = conta.CreditarValor(100, "teste");
//
//        Assert.Single(sut.Extrato);
//
//        EntradaExtrato extrato = sut.Extrato.Peek();
//        Assert.Equal("teste", extrato.Descricao);
//        Assert.Equal((uint)100, extrato.Valor);
//    }

    [Fact]
    public void Deve_ter_maximo_10_entradas_no_extrato()
    {
        Conta conta = new()
        {
            Id = 1,
            Saldo = 100,
            Limite = 100000
        };

        for (int i = 0; i < 15; i++)
            conta.CreditarValor(100, "teste");

        for (int i = 0; i < 15; i++)
            conta.DebitarValor(200, "teste");

        Assert.Equal(10, conta.Extrato.Count);
    }

}
