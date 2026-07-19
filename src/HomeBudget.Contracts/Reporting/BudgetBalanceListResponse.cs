namespace HomeBudget.Contracts.Reporting;

/// <summary>
/// Represents a list of monthly budget balances.
/// </summary>
/// <param name="Items">The budget balances returned by the query.</param>
public sealed record BudgetBalanceListResponse(IReadOnlyCollection<BudgetBalanceResponse> Items);
