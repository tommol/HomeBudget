using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Planning;

public sealed record CategoryAllocationAddedEvent(
    BudgetPlanId BudgetPlanId,
    CategoryAllocationId CategoryAllocationId,
    BudgetCategoryId CategoryId,
    Money Amount,
    CategoryAllocationFlexibility Flexibility,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
