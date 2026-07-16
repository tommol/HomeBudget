using HomeBudget.Domain.Kernel;

namespace HomeBudget.Domain.Planning;

public sealed record BudgetPlanStatusChangedEvent(
    BudgetPlanId BudgetPlanId,
    BudgetPlanStatus PreviousStatus,
    BudgetPlanStatus NewStatus,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
