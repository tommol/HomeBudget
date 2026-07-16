using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Planning.RemoveSavingContribution;

/// <summary>
/// Represents a command that removes a saving contribution from a budget plan.
/// </summary>
/// <param name="BudgetPlanId">The identifier of the budget plan.</param>
/// <param name="SavingContributionId">The identifier of the saving contribution to remove.</param>
public sealed record RemoveSavingContributionCommand(
    Guid BudgetPlanId,
    Guid SavingContributionId) : ICommand;
