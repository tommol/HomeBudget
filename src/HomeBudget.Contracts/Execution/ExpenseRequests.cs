namespace HomeBudget.Contracts.Execution;

/// <summary>
/// Represents a request to add an expense to an executed budget.
/// </summary>
/// <param name="CategoryId">The identifier of the expense category.</param>
/// <param name="Title">The expense title.</param>
/// <param name="Amount">The expense amount.</param>
/// <param name="CurrencyCode">The currency code of the expense amount.</param>
/// <param name="OccurredDate">The date when the expense occurred.</param>
/// <param name="ConvertedAmount">The amount converted to the budget default currency, when needed.</param>
/// <param name="ConversionDate">The date of the currency conversion, when needed.</param>
public sealed record AddExpenseRequest(
    Guid CategoryId,
    string Title,
    decimal Amount,
    string CurrencyCode,
    DateOnly OccurredDate,
    decimal? ConvertedAmount = null,
    DateOnly? ConversionDate = null);

/// <summary>
/// Represents a response returned after adding an expense.
/// </summary>
/// <param name="Id">The created expense identifier.</param>
public sealed record AddExpenseResponse(Guid Id);

/// <summary>
/// Represents a request to change an expense amount.
/// </summary>
/// <param name="Amount">The new expense amount.</param>
/// <param name="CurrencyCode">The currency code of the new expense amount.</param>
/// <param name="ConvertedAmount">The amount converted to the budget default currency, when needed.</param>
/// <param name="ConversionDate">The date of the currency conversion, when needed.</param>
public sealed record ChangeExpenseAmountRequest(
    decimal Amount,
    string CurrencyCode,
    decimal? ConvertedAmount = null,
    DateOnly? ConversionDate = null);

/// <summary>
/// Represents a request to change an expense category.
/// </summary>
/// <param name="CategoryId">The identifier of the new expense category.</param>
public sealed record ChangeExpenseCategoryRequest(Guid CategoryId);

/// <summary>
/// Represents a request to change an expense title.
/// </summary>
/// <param name="Title">The new expense title.</param>
public sealed record ChangeExpenseTitleRequest(string Title);

/// <summary>
/// Represents a request to change an expense occurred date.
/// </summary>
/// <param name="OccurredDate">The new occurred date.</param>
public sealed record ChangeExpenseOccurredDateRequest(DateOnly OccurredDate);

/// <summary>
/// Represents a request to remove an expense from an executed budget.
/// </summary>
/// <param name="RemovalReason">The reason why the expense is removed.</param>
public sealed record RemoveExpenseRequest(string RemovalReason);
