using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Execution.RemoveIncome;

/// <summary>
/// Represents a command that removes income from an executed budget.
/// </summary>
/// <param name="OwnerId">The identifier of the budget owner.</param>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="IncomeId">The identifier of the income to remove.</param>
/// <param name="RemovalReason">The reason why the income is removed.</param>
public sealed record RemoveIncomeCommand(
    Guid OwnerId,
    Guid BudgetId,
    Guid IncomeId,
    string RemovalReason) : ICommand;
