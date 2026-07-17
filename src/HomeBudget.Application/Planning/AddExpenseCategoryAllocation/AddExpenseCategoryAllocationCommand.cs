using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Planning.AddExpenseCategoryAllocation;

/// <summary>
/// Represents a command that adds an expense category allocation to a budget plan.
/// </summary>
/// <param name="OwnerId">The identifier of the budget plan owner.</param>
/// <param name="BudgetPlanId">The identifier of the budget plan.</param>
/// <param name="CategoryId">The identifier of the expense category to allocate.</param>
/// <param name="Amount">The amount to allocate in the budget plan default currency.</param>
/// <param name="Flexibility">The flexibility level of the allocation.</param>
public sealed record AddExpenseCategoryAllocationCommand(
    Guid OwnerId,
    Guid BudgetPlanId,
    Guid CategoryId,
    decimal Amount,
    string Flexibility) : ICommand<Guid>;
