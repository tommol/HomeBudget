using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Planning.ChangeExpenseCategoryAllocationAmount;

/// <summary>
/// Represents a command that changes the amount of an expense category allocation.
/// </summary>
/// <param name="OwnerId">The identifier of the budget plan owner.</param>
/// <param name="BudgetPlanId">The identifier of the budget plan.</param>
/// <param name="CategoryAllocationId">The identifier of the category allocation to update.</param>
/// <param name="Amount">The new allocation amount in the budget plan default currency.</param>
public sealed record ChangeExpenseCategoryAllocationAmountCommand(
    Guid OwnerId,
    Guid BudgetPlanId,
    Guid CategoryAllocationId,
    decimal Amount) : ICommand;
