using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Planning;

/// <summary>
/// Represents the event raised when a saving contribution amount changes.
/// </summary>
/// <param name="BudgetPlanId">The identifier of the budget plan.</param>
/// <param name="SavingContributionId">The identifier of the saving contribution.</param>
/// <param name="CategoryId">The identifier of the saving category.</param>
/// <param name="PreviousAmount">The previous contribution amount.</param>
/// <param name="NewAmount">The new contribution amount.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
public sealed record SavingContributionAmountChangedEvent(
    BudgetPlanId BudgetPlanId,
    SavingContributionId SavingContributionId,
    BudgetCategoryId CategoryId,
    Money PreviousAmount,
    Money NewAmount,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
