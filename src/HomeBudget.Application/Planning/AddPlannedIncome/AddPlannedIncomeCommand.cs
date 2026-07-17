using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Planning.AddPlannedIncome;

/// <summary>
/// Represents a command that adds planned income to a budget plan.
/// </summary>
/// <param name="OwnerId">The identifier of the budget plan owner.</param>
/// <param name="BudgetPlanId">The identifier of the budget plan.</param>
/// <param name="CategoryId">The identifier of the income category.</param>
/// <param name="Title">The planned income title.</param>
/// <param name="Amount">The planned income amount.</param>
/// <param name="CurrencyCode">The currency code of the planned income amount.</param>
/// <param name="ExpectedDate">The date when the income is expected.</param>
/// <param name="ConvertedAmount">The amount converted to the plan default currency, when needed.</param>
/// <param name="ConversionDate">The date of the currency conversion, when needed.</param>
public sealed record AddPlannedIncomeCommand(
    Guid OwnerId,
    Guid BudgetPlanId,
    Guid CategoryId,
    string Title,
    decimal Amount,
    string CurrencyCode,
    DateOnly ExpectedDate,
    decimal? ConvertedAmount = null,
    DateOnly? ConversionDate = null) : ICommand<Guid>;
