namespace HomeBudget.Contracts.Planning;

/// <summary>
/// Represents a response returned after adding planned income.
/// </summary>
/// <param name="Id">The created planned income identifier.</param>
public sealed record AddPlannedIncomeResponse(Guid Id);
