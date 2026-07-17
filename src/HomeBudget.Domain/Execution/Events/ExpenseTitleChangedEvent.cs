using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents the event raised when an expense title changes.
/// </summary>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="ExpenseId">The identifier of the expense.</param>
/// <param name="CategoryId">The identifier of the expense category.</param>
/// <param name="PreviousTitle">The previous expense title.</param>
/// <param name="NewTitle">The new expense title.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
public sealed record ExpenseTitleChangedEvent(
    BudgetId BudgetId,
    ExpenseId ExpenseId,
    BudgetCategoryId CategoryId,
    string PreviousTitle,
    string NewTitle,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
