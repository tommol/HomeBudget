using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents the event raised when an expense is added to a budget.
/// </summary>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="ExpenseId">The identifier of the added expense.</param>
/// <param name="CategoryId">The identifier of the expense category.</param>
/// <param name="Title">The expense title.</param>
/// <param name="Amount">The original expense amount.</param>
/// <param name="ConvertedAmount">The amount converted to the budget default currency, when applicable.</param>
/// <param name="ConversionDate">The date of the currency conversion, when applicable.</param>
/// <param name="OccurredDate">The date when the expense occurred.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
public sealed record ExpenseAddedEvent(
    BudgetId BudgetId,
    ExpenseId ExpenseId,
    BudgetCategoryId CategoryId,
    string Title,
    Money Amount,
    Money? ConvertedAmount,
    DateOnly? ConversionDate,
    DateOnly OccurredDate,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
