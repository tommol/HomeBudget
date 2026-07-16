using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Planning.ChangeSavingContributionAmount;

/// <summary>
/// Represents a command that changes the amount of a saving contribution.
/// </summary>
/// <param name="BudgetPlanId">The identifier of the budget plan.</param>
/// <param name="SavingContributionId">The identifier of the saving contribution to update.</param>
/// <param name="Amount">The new contribution amount in the budget plan default currency.</param>
public sealed record ChangeSavingContributionAmountCommand(
    Guid BudgetPlanId,
    Guid SavingContributionId,
    decimal Amount) : ICommand;
