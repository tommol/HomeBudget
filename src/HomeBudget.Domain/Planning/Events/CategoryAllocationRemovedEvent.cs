using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Planning;

/// <summary>
/// Represents the event raised when an expense category allocation is removed from a budget plan.
/// </summary>
/// <param name="BudgetPlanId">The identifier of the budget plan.</param>
/// <param name="CategoryAllocationId">The identifier of the removed category allocation.</param>
/// <param name="CategoryId">The identifier of the allocated category.</param>
/// <param name="Amount">The removed allocated amount.</param>
/// <param name="Flexibility">The flexibility level of the removed allocation.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
public sealed record CategoryAllocationRemovedEvent(
    BudgetPlanId BudgetPlanId,
    CategoryAllocationId CategoryAllocationId,
    BudgetCategoryId CategoryId,
    Money Amount,
    CategoryAllocationFlexibility Flexibility,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
