using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Planning.ChangeExpenseCategoryAllocationFlexibility;

/// <summary>
/// Represents a command that changes the flexibility of an expense category allocation.
/// </summary>
/// <param name="BudgetPlanId">The identifier of the budget plan.</param>
/// <param name="CategoryAllocationId">The identifier of the category allocation to update.</param>
/// <param name="Flexibility">The new flexibility level.</param>
public sealed record ChangeExpenseCategoryAllocationFlexibilityCommand(
    Guid BudgetPlanId,
    Guid CategoryAllocationId,
    string Flexibility) : ICommand;
