using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Planning;

/// <summary>
/// Represents the event raised when an expense category allocation flexibility changes.
/// </summary>
/// <param name="BudgetPlanId">The identifier of the budget plan.</param>
/// <param name="CategoryAllocationId">The identifier of the category allocation.</param>
/// <param name="CategoryId">The identifier of the allocated category.</param>
/// <param name="PreviousFlexibility">The previous flexibility level.</param>
/// <param name="NewFlexibility">The new flexibility level.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
public sealed record CategoryAllocationFlexibilityChangedEvent(
    BudgetPlanId BudgetPlanId,
    CategoryAllocationId CategoryAllocationId,
    BudgetCategoryId CategoryId,
    CategoryAllocationFlexibility PreviousFlexibility,
    CategoryAllocationFlexibility NewFlexibility,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
