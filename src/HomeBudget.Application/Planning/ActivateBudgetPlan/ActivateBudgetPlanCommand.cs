using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Planning.ActivateBudgetPlan;

/// <summary>
/// Represents a command that activates a budget plan.
/// </summary>
/// <param name="OwnerId">The identifier of the budget plan owner.</param>
/// <param name="BudgetPlanId">The identifier of the budget plan to activate.</param>
public sealed record ActivateBudgetPlanCommand(
    Guid OwnerId,
    Guid BudgetPlanId) : ICommand;
