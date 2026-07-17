using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Execution.ChangeIncomeCategory;

/// <summary>
/// Represents a command that changes an income category.
/// </summary>
/// <param name="OwnerId">The identifier of the budget owner.</param>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="IncomeId">The identifier of the income to update.</param>
/// <param name="CategoryId">The identifier of the new income category.</param>
public sealed record ChangeIncomeCategoryCommand(
    Guid OwnerId,
    Guid BudgetId,
    Guid IncomeId,
    Guid CategoryId) : ICommand;
