using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Execution.ChangeIncomeTitle;

/// <summary>
/// Represents a command that changes an income title.
/// </summary>
/// <param name="OwnerId">The identifier of the budget owner.</param>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="IncomeId">The identifier of the income to update.</param>
/// <param name="Title">The new income title.</param>
public sealed record ChangeIncomeTitleCommand(
    Guid OwnerId,
    Guid BudgetId,
    Guid IncomeId,
    string Title) : ICommand;
