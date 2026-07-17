namespace HomeBudget.Contracts.Planning;

/// <summary>
/// Represents a request to change an expense category allocation amount.
/// </summary>
/// <param name="Amount">The new allocation amount in the budget plan default currency.</param>
public sealed record ChangeExpenseCategoryAllocationAmountRequest(decimal Amount);
