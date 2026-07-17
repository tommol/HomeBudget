using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Execution.ChangeSavingTitle;

/// <summary>
/// Represents a command that changes a saving title.
/// </summary>
/// <param name="OwnerId">The identifier of the budget owner.</param>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="SavingId">The identifier of the saving to update.</param>
/// <param name="Title">The new saving title.</param>
public sealed record ChangeSavingTitleCommand(
    Guid OwnerId,
    Guid BudgetId,
    Guid SavingId,
    string Title) : ICommand;
