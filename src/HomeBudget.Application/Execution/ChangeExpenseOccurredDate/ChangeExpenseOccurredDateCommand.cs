using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Execution.ChangeExpenseOccurredDate;

/// <summary>
/// Represents a command that changes an expense occurred date.
/// </summary>
/// <param name="OwnerId">The identifier of the budget owner.</param>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="ExpenseId">The identifier of the expense to update.</param>
/// <param name="OccurredDate">The new occurred date.</param>
public sealed record ChangeExpenseOccurredDateCommand(
    Guid OwnerId,
    Guid BudgetId,
    Guid ExpenseId,
    DateOnly OccurredDate) : ICommand;
