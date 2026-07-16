using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Planning;

/// <summary>
/// Represents the event raised when a saving contribution is added to a budget plan.
/// </summary>
/// <param name="BudgetPlanId">The identifier of the budget plan.</param>
/// <param name="SavingContributionId">The identifier of the added saving contribution.</param>
/// <param name="CategoryId">The identifier of the saving category.</param>
/// <param name="Amount">The contribution amount.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
public sealed record SavingContributionAddedEvent(
    BudgetPlanId BudgetPlanId,
    SavingContributionId SavingContributionId,
    BudgetCategoryId CategoryId,
    Money Amount,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
