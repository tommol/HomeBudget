namespace HomeBudget.Contracts.Planning;

/// <summary>
/// Represents a request to add a saving contribution to a budget plan.
/// </summary>
/// <param name="CategoryId">The identifier of the saving category.</param>
/// <param name="Amount">The contribution amount in the budget plan default currency.</param>
public sealed record AddSavingContributionRequest(
    Guid CategoryId,
    decimal Amount);
