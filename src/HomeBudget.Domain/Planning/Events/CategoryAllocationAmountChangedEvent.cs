using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Planning;

/// <summary>
/// Represents the event raised when an expense category allocation amount changes.
/// </summary>
/// <param name="BudgetPlanId">The identifier of the budget plan.</param>
/// <param name="CategoryAllocationId">The identifier of the category allocation.</param>
/// <param name="CategoryId">The identifier of the allocated category.</param>
/// <param name="PreviousAmount">The previous allocated amount.</param>
/// <param name="NewAmount">The new allocated amount.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
public sealed record CategoryAllocationAmountChangedEvent(
    BudgetPlanId BudgetPlanId,
    CategoryAllocationId CategoryAllocationId,
    BudgetCategoryId CategoryId,
    Money PreviousAmount,
    Money NewAmount,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
