using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Execution.ChangeExpenseTitle;

/// <summary>
/// Represents a command that changes an expense title.
/// </summary>
/// <param name="OwnerId">The identifier of the budget owner.</param>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="ExpenseId">The identifier of the expense to update.</param>
/// <param name="Title">The new expense title.</param>
public sealed record ChangeExpenseTitleCommand(
    Guid OwnerId,
    Guid BudgetId,
    Guid ExpenseId,
    string Title) : ICommand;
