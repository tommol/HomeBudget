using HomeBudget.Domain.Kernel;

namespace HomeBudget.Domain.Shared;

/// <summary>
/// Represents a monetary amount in a specific currency.
/// </summary>
public sealed class Money : ValueObject
{
    /// <summary>
    /// Gets the amount of money.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the currency of the money.
    /// </summary>
    public Currency Currency { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> class with the specified amount and currency.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The currency of the monetary amount.</param>
    public Money(decimal amount, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Gets a value indicating whether the amount is equal to zero.
    /// </summary>
    public bool IsZero => Amount == 0m;

    /// <summary>
    /// Creates zero money for the specified currency.
    /// </summary>
    /// <param name="currency">The currency for the zero amount.</param>
    /// <returns>A money instance with zero amount.</returns>
    public static Money Zero(Currency currency) => new(0m, currency);

    /// <summary>
    /// Adds two money values with the same currency.
    /// </summary>
    /// <param name="left">The first money value.</param>
    /// <param name="right">The second money value.</param>
    /// <returns>The sum of both money values.</returns>
    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);

        return new Money(left.Amount + right.Amount, left.Currency);
    }

    /// <summary>
    /// Subtracts two money values with the same currency.
    /// </summary>
    /// <param name="left">The first money value.</param>
    /// <param name="right">The second money value.</param>
    /// <returns>The difference between both money values.</returns>
    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);

        return new Money(left.Amount - right.Amount, left.Currency);
    }

    /// <summary>
    /// Multiplies money by a decimal multiplier.
    /// </summary>
    /// <param name="money">The money value.</param>
    /// <param name="multiplier">The decimal multiplier.</param>
    /// <returns>The multiplied money value.</returns>
    public static Money operator *(Money money, decimal multiplier)
    {
        ArgumentNullException.ThrowIfNull(money);

        return new Money(money.Amount * multiplier, money.Currency);
    }

    /// <summary>
    /// Multiplies money by a decimal multiplier.
    /// </summary>
    /// <param name="multiplier">The decimal multiplier.</param>
    /// <param name="money">The money value.</param>
    /// <returns>The multiplied money value.</returns>
    public static Money operator *(decimal multiplier, Money money) => money * multiplier;

    /// <summary>
    /// Divides money by a decimal divisor.
    /// </summary>
    /// <param name="money">The money value.</param>
    /// <param name="divisor">The decimal divisor.</param>
    /// <returns>The divided money value.</returns>
    public static Money operator /(Money money, decimal divisor)
    {
        ArgumentNullException.ThrowIfNull(money);

        return new Money(money.Amount / divisor, money.Currency);
    }

    /// <summary>
    /// Gets the component values used to determine money equality.
    /// </summary>
    /// <returns>The amount and currency of the money value.</returns>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    /// <summary>
    /// Returns the string representation of the money, which includes the amount and the currency code.
    /// </summary>
    /// <returns>The string representation of the money.</returns>
    public override string ToString() => $"{Amount} {Currency}";

    private static void EnsureSameCurrency(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        if (!left.Currency.Equals(right.Currency))
        {
            throw new InvalidOperationException("Cannot operate on money with different currencies.");
        }
    }
}
