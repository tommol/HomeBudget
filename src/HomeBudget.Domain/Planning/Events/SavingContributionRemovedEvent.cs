using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Planning;

/// <summary>
/// Represents the event raised when a saving contribution is removed from a budget plan.
/// </summary>
/// <param name="BudgetPlanId">The identifier of the budget plan.</param>
/// <param name="SavingContributionId">The identifier of the removed saving contribution.</param>
/// <param name="CategoryId">The identifier of the saving category.</param>
/// <param name="Amount">The removed contribution amount.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
public sealed record SavingContributionRemovedEvent(
    BudgetPlanId BudgetPlanId,
    SavingContributionId SavingContributionId,
    BudgetCategoryId CategoryId,
    Money Amount,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
