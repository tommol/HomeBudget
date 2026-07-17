using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Execution.ChangeIncomeOccurredDate;

/// <summary>
/// Represents a command that changes an income occurred date.
/// </summary>
/// <param name="OwnerId">The identifier of the budget owner.</param>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="IncomeId">The identifier of the income to update.</param>
/// <param name="OccurredDate">The new occurred date.</param>
public sealed record ChangeIncomeOccurredDateCommand(
    Guid OwnerId,
    Guid BudgetId,
    Guid IncomeId,
    DateOnly OccurredDate) : ICommand;
