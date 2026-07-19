using HomeBudget.Application.Abstractions;
using HomeBudget.Contracts.Reporting;

namespace HomeBudget.Application.Reporting.GetCurrentBudgetBalance;

/// <summary>
/// Represents a query that gets the budget balance for the server's current period.
/// </summary>
public sealed record GetCurrentBudgetBalanceQuery(Guid OwnerId) : IQuery<BudgetBalanceResponse>;
