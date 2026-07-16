using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Planning;

/// <summary>
/// Represents the event raised when planned income is added to a budget plan.
/// </summary>
/// <param name="BudgetPlanId">The identifier of the budget plan.</param>
/// <param name="PlannedIncomeId">The identifier of the added planned income.</param>
/// <param name="CategoryId">The identifier of the income category.</param>
/// <param name="Title">The planned income title.</param>
/// <param name="Amount">The original planned income amount.</param>
/// <param name="ConvertedAmount">The amount converted to the budget plan default currency, when applicable.</param>
/// <param name="ConversionDate">The date of the currency conversion, when applicable.</param>
/// <param name="ExpectedDate">The date when the income is expected.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
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
