using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Execution.ChangeSavingCategory;

/// <summary>
/// Represents a command that changes a saving category.
/// </summary>
/// <param name="OwnerId">The identifier of the budget owner.</param>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="SavingId">The identifier of the saving to update.</param>
/// <param name="CategoryId">The identifier of the new saving category.</param>
public sealed record ChangeSavingCategoryCommand(
    Guid OwnerId,
    Guid BudgetId,
    Guid SavingId,
    Guid CategoryId) : ICommand;
