namespace HomeBudget.Contracts.Planning;

/// <summary>
/// Represents a request to add planned income to a budget plan.
/// </summary>
/// <param name="CategoryId">The identifier of the income category.</param>
/// <param name="Title">The planned income title.</param>
/// <param name="Amount">The planned income amount.</param>
/// <param name="CurrencyCode">The currency code of the planned income amount.</param>
/// <param name="ExpectedDate">The date when the income is expected.</param>
/// <param name="ConvertedAmount">The amount converted to the budget plan default currency, when needed.</param>
/// <param name="ConversionDate">The date of the currency conversion, when needed.</param>
public sealed record AddPlannedIncomeRequest(
    Guid CategoryId,
    string Title,
    decimal Amount,
    string CurrencyCode,
    DateOnly ExpectedDate,
    decimal? ConvertedAmount = null,
    DateOnly? ConversionDate = null);
