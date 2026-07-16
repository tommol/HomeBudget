using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Planning;

public sealed record SavingContributionAddedEvent(
    BudgetPlanId BudgetPlanId,
    SavingContributionId SavingContributionId,
    BudgetCategoryId CategoryId,
    Money Amount,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
