namespace HomeBudget.Contracts.Planning;

/// <summary>
/// Represents a request to add an expense category allocation to a budget plan.
/// </summary>
/// <param name="CategoryId">The identifier of the expense category to allocate.</param>
/// <param name="Amount">The amount to allocate in the budget plan default currency.</param>
/// <param name="Flexibility">The flexibility level of the allocation.</param>
public sealed record AddExpenseCategoryAllocationRequest(
    Guid CategoryId,
    decimal Amount,
    string Flexibility);
