namespace HomeBudget.Contracts.Planning;

/// <summary>
/// Represents a response returned after copying a budget plan.
/// </summary>
/// <param name="Id">The copied budget plan identifier.</param>
public sealed record CopyBudgetPlanResponse(Guid Id);
