using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Execution.ChangeExpenseCategory;

/// <summary>
/// Represents a command that changes an expense category.
/// </summary>
/// <param name="OwnerId">The identifier of the budget owner.</param>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="ExpenseId">The identifier of the expense to update.</param>
/// <param name="CategoryId">The identifier of the new expense category.</param>
public sealed record ChangeExpenseCategoryCommand(
    Guid OwnerId,
    Guid BudgetId,
    Guid ExpenseId,
    Guid CategoryId) : ICommand;
