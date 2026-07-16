using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Tests.Shared;

public sealed class MoneyTests
{
    [Fact]
    public void Constructor_SetsAmountAndCurrency()
    {
        var money = new Money(123.45m, Currency.PLN);

        Assert.Equal(123.45m, money.Amount);
        Assert.Equal(Currency.PLN, money.Currency);
    }

    [Fact]
    public void Constructor_Throws_WhenCurrencyIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new Money(123.45m, null!));
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenAmountAndCurrencyMatch()
    {
        var left = new Money(123.45m, Currency.PLN);
        var right = new Money(123.45m, new Currency("pln"));

        Assert.Equal(left, right);
        Assert.True(left == right);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenCurrencyDiffers()
    {
        var left = new Money(123.45m, Currency.PLN);
        var right = new Money(123.45m, Currency.EUR);

        Assert.NotEqual(left, right);
    }

    [Fact]
    public void Zero_ReturnsMoneyWithZeroAmount()
    {
        var money = Money.Zero(Currency.PLN);

        Assert.Equal(0m, money.Amount);
        Assert.Equal(Currency.PLN, money.Currency);
        Assert.True(money.IsZero);
    }

    [Fact]
    public void IsZero_ReturnsFalse_WhenAmountIsNotZero()
    {
        var money = new Money(1m, Currency.PLN);

        Assert.False(money.IsZero);
    }

    [Fact]
    public void Add_ReturnsSum_WhenCurrenciesMatch()
    {
        var left = new Money(100m, Currency.PLN);
        var right = new Money(50m, Currency.PLN);

        var result = left + right;

        Assert.Equal(new Money(150m, Currency.PLN), result);
    }

    [Fact]
    public void Add_Throws_WhenCurrenciesDiffer()
    {
        var left = new Money(100m, Currency.PLN);
        var right = new Money(50m, Currency.EUR);

        Assert.Throws<InvalidOperationException>(() => left + right);
    }

    [Fact]
    public void Subtract_ReturnsDifference_WhenCurrenciesMatch()
    {
        var left = new Money(100m, Currency.PLN);
        var right = new Money(50m, Currency.PLN);

        var result = left - right;

        Assert.Equal(new Money(50m, Currency.PLN), result);
    }

    [Fact]
    public void Subtract_Throws_WhenCurrenciesDiffer()
    {
        var left = new Money(100m, Currency.PLN);
        var right = new Money(50m, Currency.EUR);

        Assert.Throws<InvalidOperationException>(() => left - right);
    }

    [Fact]
    public void Multiply_ReturnsMultipliedMoney()
    {
        var money = new Money(100m, Currency.PLN);

        var result = money * 1.5m;

        Assert.Equal(new Money(150m, Currency.PLN), result);
    }

    [Fact]
    public void Multiply_ReturnsMultipliedMoney_WhenMultiplierIsFirstOperand()
    {
        var money = new Money(100m, Currency.PLN);

        var result = 1.5m * money;

        Assert.Equal(new Money(150m, Currency.PLN), result);
    }

    [Fact]
    public void Divide_ReturnsDividedMoney()
    {
        var money = new Money(100m, Currency.PLN);

        var result = money / 4m;

        Assert.Equal(new Money(25m, Currency.PLN), result);
    }
}
