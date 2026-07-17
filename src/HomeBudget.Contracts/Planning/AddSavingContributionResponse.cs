namespace HomeBudget.Contracts.Planning;

/// <summary>
/// Represents a response returned after adding a saving contribution.
/// </summary>
/// <param name="Id">The created saving contribution identifier.</param>
public sealed record AddSavingContributionResponse(Guid Id);
