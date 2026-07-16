using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Planning.AddSavingContribution;

/// <summary>
/// Represents a command that adds a saving contribution to a budget plan.
/// </summary>
/// <param name="BudgetPlanId">The identifier of the budget plan.</param>
/// <param name="CategoryId">The identifier of the saving category.</param>
/// <param name="Amount">The contribution amount in the budget plan default currency.</param>
public sealed record AddSavingContributionCommand(
    Guid BudgetPlanId,
    Guid CategoryId,
    decimal Amount) : ICommand<Guid>;
