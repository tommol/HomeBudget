using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Planning.RemoveExpenseCategoryAllocation;

/// <summary>
/// Represents a command that removes an expense category allocation from a budget plan.
/// </summary>
/// <param name="OwnerId">The identifier of the budget plan owner.</param>
/// <param name="BudgetPlanId">The identifier of the budget plan.</param>
/// <param name="CategoryAllocationId">The identifier of the category allocation to remove.</param>
public sealed record RemoveExpenseCategoryAllocationCommand(
    Guid OwnerId,
    Guid BudgetPlanId,
    Guid CategoryAllocationId) : ICommand;
