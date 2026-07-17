using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents the event raised when an expense category changes.
/// </summary>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="ExpenseId">The identifier of the expense.</param>
/// <param name="PreviousCategoryId">The previous expense category identifier.</param>
/// <param name="NewCategoryId">The new expense category identifier.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
public sealed record ExpenseCategoryChangedEvent(
    BudgetId BudgetId,
    ExpenseId ExpenseId,
    BudgetCategoryId PreviousCategoryId,
    BudgetCategoryId NewCategoryId,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
