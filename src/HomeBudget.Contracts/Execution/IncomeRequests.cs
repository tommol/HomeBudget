namespace HomeBudget.Contracts.Execution;

/// <summary>
/// Represents a request to add income to an executed budget.
/// </summary>
/// <param name="CategoryId">The identifier of the income category.</param>
/// <param name="Title">The income title.</param>
/// <param name="Amount">The income amount.</param>
/// <param name="CurrencyCode">The currency code of the income amount.</param>
/// <param name="OccurredDate">The date when the income occurred.</param>
/// <param name="ConvertedAmount">The amount converted to the budget default currency, when needed.</param>
/// <param name="ConversionDate">The date of the currency conversion, when needed.</param>
public sealed record AddIncomeRequest(
    Guid CategoryId,
    string Title,
    decimal Amount,
    string CurrencyCode,
    DateOnly OccurredDate,
    decimal? ConvertedAmount = null,
    DateOnly? ConversionDate = null);

/// <summary>
/// Represents a response returned after adding income.
/// </summary>
/// <param name="Id">The created income identifier.</param>
public sealed record AddIncomeResponse(Guid Id);

/// <summary>
/// Represents a request to change an income amount.
/// </summary>
/// <param name="Amount">The new income amount.</param>
/// <param name="CurrencyCode">The currency code of the new income amount.</param>
/// <param name="ConvertedAmount">The amount converted to the budget default currency, when needed.</param>
/// <param name="ConversionDate">The date of the currency conversion, when needed.</param>
public sealed record ChangeIncomeAmountRequest(
    decimal Amount,
    string CurrencyCode,
    decimal? ConvertedAmount = null,
    DateOnly? ConversionDate = null);

/// <summary>
/// Represents a request to change an income category.
/// </summary>
/// <param name="CategoryId">The identifier of the new income category.</param>
public sealed record ChangeIncomeCategoryRequest(Guid CategoryId);

/// <summary>
/// Represents a request to change an income title.
/// </summary>
/// <param name="Title">The new income title.</param>
public sealed record ChangeIncomeTitleRequest(string Title);

/// <summary>
/// Represents a request to change an income occurred date.
/// </summary>
/// <param name="OccurredDate">The new occurred date.</param>
public sealed record ChangeIncomeOccurredDateRequest(DateOnly OccurredDate);

/// <summary>
/// Represents a request to remove income from an executed budget.
/// </summary>
/// <param name="RemovalReason">The reason why the income is removed.</param>
public sealed record RemoveIncomeRequest(string RemovalReason);
