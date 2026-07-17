using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents the event raised when a saving category changes.
/// </summary>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="SavingId">The identifier of the saving.</param>
/// <param name="PreviousCategoryId">The previous saving category identifier.</param>
/// <param name="NewCategoryId">The new saving category identifier.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
public sealed record SavingCategoryChangedEvent(
    BudgetId BudgetId,
    SavingId SavingId,
    BudgetCategoryId PreviousCategoryId,
    BudgetCategoryId NewCategoryId,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
