using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Planning;

public sealed record PlannedIncomeAddedEvent(
    BudgetPlanId BudgetPlanId,
    PlannedIncomeId PlannedIncomeId,
    BudgetCategoryId CategoryId,
    string Title,
    Money Amount,
    Money? ConvertedAmount,
    DateOnly? ConversionDate,
    DateOnly ExpectedDate,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
