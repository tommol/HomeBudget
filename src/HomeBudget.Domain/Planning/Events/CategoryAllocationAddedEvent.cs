using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Planning;

/// <summary>
/// Represents the event raised when an expense category allocation is added to a budget plan.
/// </summary>
/// <param name="BudgetPlanId">The identifier of the budget plan.</param>
/// <param name="CategoryAllocationId">The identifier of the added category allocation.</param>
/// <param name="CategoryId">The identifier of the allocated category.</param>
/// <param name="Amount">The allocated amount.</param>
/// <param name="Flexibility">The flexibility level of the allocation.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
public sealed record CategoryAllocationAddedEvent(
    BudgetPlanId BudgetPlanId,
    CategoryAllocationId CategoryAllocationId,
    BudgetCategoryId CategoryId,
    Money Amount,
    CategoryAllocationFlexibility Flexibility,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
