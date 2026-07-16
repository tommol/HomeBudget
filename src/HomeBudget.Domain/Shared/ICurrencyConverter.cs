namespace HomeBudget.Domain.Shared;

public interface ICurrencyConverter
{
    ValueTask<Money> ConvertAsync(
        DateOnly rateDate,
        Money source,
        Currency targetCurrency,
        CancellationToken cancellationToken = default);
}
