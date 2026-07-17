using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Execution.ChangeExpenseAmount;

/// <summary>
/// Represents a command that changes an expense amount.
/// </summary>
/// <param name="OwnerId">The identifier of the budget owner.</param>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="ExpenseId">The identifier of the expense to update.</param>
/// <param name="Amount">The new expense amount.</param>
/// <param name="CurrencyCode">The currency code of the new expense amount.</param>
/// <param name="ConvertedAmount">The amount converted to the budget default currency, when needed.</param>
/// <param name="ConversionDate">The date of the currency conversion, when needed.</param>
public sealed record ChangeExpenseAmountCommand(
    Guid OwnerId,
    Guid BudgetId,
    Guid ExpenseId,
    decimal Amount,
    string CurrencyCode,
    decimal? ConvertedAmount = null,
    DateOnly? ConversionDate = null) : ICommand;
