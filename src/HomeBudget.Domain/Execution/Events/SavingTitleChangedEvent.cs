using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents the event raised when a saving title changes.
/// </summary>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="SavingId">The identifier of the saving.</param>
/// <param name="CategoryId">The identifier of the saving category.</param>
/// <param name="PreviousTitle">The previous saving title.</param>
/// <param name="NewTitle">The new saving title.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
public sealed record SavingTitleChangedEvent(
    BudgetId BudgetId,
    SavingId SavingId,
    BudgetCategoryId CategoryId,
    string PreviousTitle,
    string NewTitle,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
