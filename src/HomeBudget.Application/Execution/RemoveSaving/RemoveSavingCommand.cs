using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Execution.RemoveSaving;

/// <summary>
/// Represents a command that removes a saving from an executed budget.
/// </summary>
/// <param name="OwnerId">The identifier of the budget owner.</param>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="SavingId">The identifier of the saving to remove.</param>
/// <param name="RemovalReason">The reason why the saving is removed.</param>
public sealed record RemoveSavingCommand(
    Guid OwnerId,
    Guid BudgetId,
    Guid SavingId,
    string RemovalReason) : ICommand;
