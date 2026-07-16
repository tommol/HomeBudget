namespace HomeBudget.Domain.Shared;

/// <summary>
/// Converts monetary amounts between currencies.
/// </summary>
public interface ICurrencyConverter
{
    /// <summary>
    /// Converts a source money value to the target currency using a rate for the specified date.
    /// </summary>
    /// <param name="rateDate">The date of the exchange rate to use.</param>
    /// <param name="source">The source money value.</param>
    /// <param name="targetCurrency">The target currency.</param>
    /// <param name="cancellationToken">A token to cancel the conversion.</param>
    /// <returns>The converted money value.</returns>
    ValueTask<Money> ConvertAsync(
        DateOnly rateDate,
        Money source,
        Currency targetCurrency,
        CancellationToken cancellationToken = default);
}
