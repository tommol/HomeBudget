using HomeBudget.Domain.Kernel;

namespace HomeBudget.Domain.Shared;

/// <summary>
/// Represents a currency using a three-letter ISO 4217 code.
/// </summary>
public sealed class Currency : ValueObject
{
    /// <summary>
    /// Gets the three-letter ISO 4217 code of the currency.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Currency"/> class with the specified three-letter ISO 4217 code.
    /// </summary>
    /// <param name="code">The three-letter ISO 4217 code of the currency.</param>
    /// <exception cref="ArgumentException">Thrown when the code is null, empty, or not a valid ISO 4217 code.</exception>
    public Currency(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Currency code is required.", nameof(code));
        }

        code = code.Trim().ToUpperInvariant();

        if (code.Length != 3)
        {
            throw new ArgumentException("Currency code must have 3 characters.", nameof(code));
        }

        if (code.Any(character => character is < 'A' or > 'Z'))
        {
            throw new ArgumentException("Currency code must contain only letters.", nameof(code));
        }

        Code = code;
    }

    /// <summary>
    /// Gets the Polish zloty currency.
    /// </summary>
    public static Currency PLN => new("PLN");

    /// <summary>
    /// Gets the euro currency.
    /// </summary>
    public static Currency EUR => new("EUR");

    /// <summary>
    /// Gets the United States dollar currency.
    /// </summary>
    public static Currency USD => new("USD");

    /// <summary>
    /// Gets the Swiss franc currency.
    /// </summary>
    public static Currency CHF => new("CHF");

    /// <summary>
    /// Gets the pound sterling currency.
    /// </summary>
    public static Currency GBP => new("GBP");

    /// <summary>
    /// Gets the component values used to determine currency equality.
    /// </summary>
    /// <returns>The currency code.</returns>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
    }

    /// <summary>
    /// Returns the string representation of the currency, which is its three-letter ISO 4217 code.
    /// </summary>
    /// <returns>The three-letter ISO 4217 code of the currency.</returns>
    public override string ToString() => Code;
}
