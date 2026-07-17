using HomeBudget.Domain.Kernel;

namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents the event raised when a budget status changes.
/// </summary>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="PreviousStatus">The previous budget status.</param>
/// <param name="NewStatus">The new budget status.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
public sealed record BudgetStatusChangedEvent(
    BudgetId BudgetId,
    BudgetStatus PreviousStatus,
    BudgetStatus NewStatus,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
