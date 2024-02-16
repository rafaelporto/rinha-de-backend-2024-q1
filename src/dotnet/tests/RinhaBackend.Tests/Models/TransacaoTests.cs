using RinhaBackend.Api.Endpoints;

namespace RinhaBackend.Tests.Transacoes;

public class TransacaoTests
{
    [Fact]
    public void Deve_ser_invalida_quando_nao_possui_valor()
    {
        Transacao transacao = new(null, 'd', "teste");
        Assert.True(transacao.IsInvalid());
    }

    [Fact]
    public void Deve_ser_invalida_quando_nao_possui_tipo()
    {
        Transacao transacao = new(10, null, "teste");
        Assert.True(transacao.IsInvalid());
    }

    [Theory]
    [MemberData(nameof(GetAllCharacters))]
    public void Deve_ser_invalida_quando_tipo_nao_eh_debito_ou_credito(char tipo)
    {
        Transacao transacao = new(10, tipo, "teste");
        Assert.True(transacao.IsInvalid());
    }

    [Fact]
    public void Deve_ser_invalida_quando_nao_possui_descricao()
    {
        Transacao transacao = new(10, 'd', null);
        Assert.True(transacao.IsInvalid());
    }

    [Fact]
    public void Deve_ser_invalida_quando_descricao_eh_maior_que_10_caracteres()
    {
        Transacao transacao = new(10, 'd', "descricao maior que 10 caracteres");
        Assert.True(transacao.IsInvalid());
    }

    public static IEnumerable<object[]> GetAllCharacters()
    {
        yield return new object[] { 'a' };
        yield return new object[] { 'b' };
        yield return new object[] { 'e' };
        yield return new object[] { 'f' };
        yield return new object[] { 'g' };
        yield return new object[] { 'h' };
        yield return new object[] { 'i' };
        yield return new object[] { 'j' };
        yield return new object[] { 'k' };
        yield return new object[] { 'l' };
        yield return new object[] { 'm' };
        yield return new object[] { 'n' };
        yield return new object[] { 'o' };
        yield return new object[] { 'p' };
        yield return new object[] { 'q' };
        yield return new object[] { 'r' };
        yield return new object[] { 's' };
        yield return new object[] { 't' };
        yield return new object[] { 'u' };
        yield return new object[] { 'v' };
        yield return new object[] { 'w' };
        yield return new object[] { 'x' };
        yield return new object[] { 'y' };
        yield return new object[] { 'z' };
    }
}
