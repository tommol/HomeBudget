namespace HomeBudget.Contracts.Planning;

/// <summary>
/// Represents a request to change a saving contribution amount.
/// </summary>
/// <param name="Amount">The new contribution amount in the budget plan default currency.</param>
public sealed record ChangeSavingContributionAmountRequest(decimal Amount);
