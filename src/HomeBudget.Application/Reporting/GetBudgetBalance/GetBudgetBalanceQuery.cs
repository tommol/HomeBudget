using HomeBudget.Application.Abstractions;
using HomeBudget.Contracts.Reporting;

namespace HomeBudget.Application.Reporting.GetBudgetBalance;

/// <summary>
/// Represents a query that gets a budget balance for a specific period.
/// </summary>
public sealed record GetBudgetBalanceQuery(Guid OwnerId, int Year, int Month) : IQuery<BudgetBalanceResponse>;
