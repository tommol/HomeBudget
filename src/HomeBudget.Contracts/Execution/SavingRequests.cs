namespace HomeBudget.Contracts.Execution;

/// <summary>
/// Represents a request to add a saving to an executed budget.
/// </summary>
/// <param name="CategoryId">The identifier of the saving category.</param>
/// <param name="Title">The saving title.</param>
/// <param name="Amount">The saving amount.</param>
/// <param name="CurrencyCode">The currency code of the saving amount.</param>
/// <param name="OccurredDate">The date when the saving occurred.</param>
/// <param name="ConvertedAmount">The amount converted to the budget default currency, when needed.</param>
/// <param name="ConversionDate">The date of the currency conversion, when needed.</param>
public sealed record AddSavingRequest(
    Guid CategoryId,
    string Title,
    decimal Amount,
    string CurrencyCode,
    DateOnly OccurredDate,
    decimal? ConvertedAmount = null,
    DateOnly? ConversionDate = null);

/// <summary>
/// Represents a response returned after adding a saving.
/// </summary>
/// <param name="Id">The created saving identifier.</param>
public sealed record AddSavingResponse(Guid Id);

/// <summary>
/// Represents a request to change a saving amount.
/// </summary>
/// <param name="Amount">The new saving amount.</param>
/// <param name="CurrencyCode">The currency code of the new saving amount.</param>
/// <param name="ConvertedAmount">The amount converted to the budget default currency, when needed.</param>
/// <param name="ConversionDate">The date of the currency conversion, when needed.</param>
public sealed record ChangeSavingAmountRequest(
    decimal Amount,
    string CurrencyCode,
    decimal? ConvertedAmount = null,
    DateOnly? ConversionDate = null);

/// <summary>
/// Represents a request to change a saving category.
/// </summary>
/// <param name="CategoryId">The identifier of the new saving category.</param>
public sealed record ChangeSavingCategoryRequest(Guid CategoryId);

/// <summary>
/// Represents a request to change a saving title.
/// </summary>
/// <param name="Title">The new saving title.</param>
public sealed record ChangeSavingTitleRequest(string Title);

/// <summary>
/// Represents a request to change a saving occurred date.
/// </summary>
/// <param name="OccurredDate">The new occurred date.</param>
public sealed record ChangeSavingOccurredDateRequest(DateOnly OccurredDate);

/// <summary>
/// Represents a request to remove a saving from an executed budget.
/// </summary>
/// <param name="RemovalReason">The reason why the saving is removed.</param>
public sealed record RemoveSavingRequest(string RemovalReason);
