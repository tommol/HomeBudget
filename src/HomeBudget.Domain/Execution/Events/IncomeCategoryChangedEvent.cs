using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents the event raised when an income category changes.
/// </summary>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="IncomeId">The identifier of the income.</param>
/// <param name="PreviousCategoryId">The previous income category identifier.</param>
/// <param name="NewCategoryId">The new income category identifier.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
public sealed record IncomeCategoryChangedEvent(
    BudgetId BudgetId,
    IncomeId IncomeId,
    BudgetCategoryId PreviousCategoryId,
    BudgetCategoryId NewCategoryId,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
