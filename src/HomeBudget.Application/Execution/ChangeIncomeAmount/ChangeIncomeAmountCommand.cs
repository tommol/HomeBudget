using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Execution.ChangeIncomeAmount;

/// <summary>
/// Represents a command that changes an income amount.
/// </summary>
/// <param name="OwnerId">The identifier of the budget owner.</param>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="IncomeId">The identifier of the income to update.</param>
/// <param name="Amount">The new income amount.</param>
/// <param name="CurrencyCode">The currency code of the new income amount.</param>
/// <param name="ConvertedAmount">The amount converted to the budget default currency, when needed.</param>
/// <param name="ConversionDate">The date of the currency conversion, when needed.</param>
public sealed record ChangeIncomeAmountCommand(
    Guid OwnerId,
    Guid BudgetId,
    Guid IncomeId,
    decimal Amount,
    string CurrencyCode,
    decimal? ConvertedAmount = null,
    DateOnly? ConversionDate = null) : ICommand;
