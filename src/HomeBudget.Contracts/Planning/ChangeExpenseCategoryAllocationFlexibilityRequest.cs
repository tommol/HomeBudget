namespace HomeBudget.Contracts.Planning;

/// <summary>
/// Represents a request to change an expense category allocation flexibility.
/// </summary>
/// <param name="Flexibility">The new flexibility level.</param>
public sealed record ChangeExpenseCategoryAllocationFlexibilityRequest(string Flexibility);
