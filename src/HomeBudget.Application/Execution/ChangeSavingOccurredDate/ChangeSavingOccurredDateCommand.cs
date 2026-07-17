using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Execution.ChangeSavingOccurredDate;

/// <summary>
/// Represents a command that changes a saving occurred date.
/// </summary>
/// <param name="OwnerId">The identifier of the budget owner.</param>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="SavingId">The identifier of the saving to update.</param>
/// <param name="OccurredDate">The new occurred date.</param>
public sealed record ChangeSavingOccurredDateCommand(
    Guid OwnerId,
    Guid BudgetId,
    Guid SavingId,
    DateOnly OccurredDate) : ICommand;
