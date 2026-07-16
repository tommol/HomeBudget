using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Tests.Shared;

public sealed class CurrencyTests
{
    [Fact]
    public void Constructor_NormalizesCode()
    {
        var currency = new Currency(" pln ");

        Assert.Equal("PLN", currency.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("PL")]
    [InlineData("PLNN")]
    [InlineData("P1N")]
    public void Constructor_Throws_WhenCodeIsInvalid(string code)
    {
        Assert.Throws<ArgumentException>(() => new Currency(code));
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenCodesMatch()
    {
        var left = new Currency("pln");
        var right = Currency.PLN;

        Assert.Equal(left, right);
        Assert.True(left == right);
    }

    [Fact]
    public void ToString_ReturnsCode()
    {
        var currency = Currency.EUR;

        Assert.Equal("EUR", currency.ToString());
    }
}
