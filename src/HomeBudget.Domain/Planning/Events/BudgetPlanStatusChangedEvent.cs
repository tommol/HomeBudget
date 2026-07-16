using HomeBudget.Domain.Kernel;

namespace HomeBudget.Domain.Planning;

/// <summary>
/// Represents the event raised when a budget plan status changes.
/// </summary>
/// <param name="BudgetPlanId">The identifier of the budget plan.</param>
/// <param name="PreviousStatus">The previous budget plan status.</param>
/// <param name="NewStatus">The new budget plan status.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
public sealed record BudgetPlanStatusChangedEvent(
    BudgetPlanId BudgetPlanId,
    BudgetPlanStatus PreviousStatus,
    BudgetPlanStatus NewStatus,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
