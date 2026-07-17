namespace HomeBudget.Contracts.Planning;

/// <summary>
/// Represents a response returned after creating a budget plan.
/// </summary>
/// <param name="Id">The created budget plan identifier.</param>
public sealed record CreateBudgetPlanResponse(Guid Id);
