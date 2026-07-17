using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Execution.ChangeSavingAmount;

/// <summary>
/// Represents a command that changes a saving amount.
/// </summary>
/// <param name="OwnerId">The identifier of the budget owner.</param>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="SavingId">The identifier of the saving to update.</param>
/// <param name="Amount">The new saving amount.</param>
/// <param name="CurrencyCode">The currency code of the new saving amount.</param>
/// <param name="ConvertedAmount">The amount converted to the budget default currency, when needed.</param>
/// <param name="ConversionDate">The date of the currency conversion, when needed.</param>
public sealed record ChangeSavingAmountCommand(
    Guid OwnerId,
    Guid BudgetId,
    Guid SavingId,
    decimal Amount,
    string CurrencyCode,
    decimal? ConvertedAmount = null,
    DateOnly? ConversionDate = null) : ICommand;
