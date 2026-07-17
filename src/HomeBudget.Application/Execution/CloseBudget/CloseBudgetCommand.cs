using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Execution.CloseBudget;

/// <summary>
/// Represents a command that closes an executed budget.
/// </summary>
/// <param name="OwnerId">The identifier of the budget owner.</param>
/// <param name="BudgetId">The identifier of the budget.</param>
public sealed record CloseBudgetCommand(Guid OwnerId, Guid BudgetId) : ICommand;
