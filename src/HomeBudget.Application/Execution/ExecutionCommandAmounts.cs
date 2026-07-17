using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Execution;

internal static class ExecutionCommandAmounts
{
    public static Money CreateAmount(decimal amount, string currencyCode)
        => new(amount, new Currency(currencyCode));

    public static Money? CreateConvertedAmount(decimal? convertedAmount, Currency defaultCurrency)
    {
        ArgumentNullException.ThrowIfNull(defaultCurrency);

        return convertedAmount is null
            ? null
            : new Money(convertedAmount.Value, defaultCurrency);
    }
}
