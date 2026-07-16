using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Planning.CloseBudgetPlan;

/// <summary>
/// Represents a command that closes a budget plan.
/// </summary>
/// <param name="BudgetPlanId">The identifier of the budget plan to close.</param>
public sealed record CloseBudgetPlanCommand(Guid BudgetPlanId) : ICommand;
