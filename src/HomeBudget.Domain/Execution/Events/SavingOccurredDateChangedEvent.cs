using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents the event raised when a saving occurred date changes.
/// </summary>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="SavingId">The identifier of the saving.</param>
/// <param name="CategoryId">The identifier of the saving category.</param>
/// <param name="PreviousOccurredDate">The previous occurred date.</param>
/// <param name="NewOccurredDate">The new occurred date.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
public sealed record SavingOccurredDateChangedEvent(
    BudgetId BudgetId,
    SavingId SavingId,
    BudgetCategoryId CategoryId,
    DateOnly PreviousOccurredDate,
    DateOnly NewOccurredDate,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
