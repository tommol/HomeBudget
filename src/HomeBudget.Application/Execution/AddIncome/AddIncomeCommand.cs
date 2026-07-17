using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Execution.AddIncome;

/// <summary>
/// Represents a command that adds income to an executed budget.
/// </summary>
/// <param name="OwnerId">The identifier of the budget owner.</param>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="CategoryId">The identifier of the income category.</param>
/// <param name="Title">The income title.</param>
/// <param name="Amount">The income amount.</param>
/// <param name="CurrencyCode">The currency code of the income amount.</param>
/// <param name="OccurredDate">The date when the income occurred.</param>
/// <param name="ConvertedAmount">The amount converted to the budget default currency, when needed.</param>
/// <param name="ConversionDate">The date of the currency conversion, when needed.</param>
public sealed record AddIncomeCommand(
    Guid OwnerId,
    Guid BudgetId,
    Guid CategoryId,
    string Title,
    decimal Amount,
    string CurrencyCode,
    DateOnly OccurredDate,
    decimal? ConvertedAmount = null,
    DateOnly? ConversionDate = null) : ICommand<Guid>;
