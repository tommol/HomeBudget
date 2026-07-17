namespace HomeBudget.Contracts.Planning;

/// <summary>
/// Represents a response returned after adding an expense category allocation.
/// </summary>
/// <param name="Id">The created expense category allocation identifier.</param>
public sealed record AddExpenseCategoryAllocationResponse(Guid Id);
