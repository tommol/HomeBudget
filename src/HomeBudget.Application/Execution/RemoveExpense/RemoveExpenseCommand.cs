using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Execution.RemoveExpense;

/// <summary>
/// Represents a command that removes an expense from an executed budget.
/// </summary>
/// <param name="OwnerId">The identifier of the budget owner.</param>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="ExpenseId">The identifier of the expense to remove.</param>
/// <param name="RemovalReason">The reason why the expense is removed.</param>
public sealed record RemoveExpenseCommand(
    Guid OwnerId,
    Guid BudgetId,
    Guid ExpenseId,
    string RemovalReason) : ICommand;
