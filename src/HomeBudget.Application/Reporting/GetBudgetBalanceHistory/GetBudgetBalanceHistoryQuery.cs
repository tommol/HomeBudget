using HomeBudget.Application.Abstractions;
using HomeBudget.Contracts.Reporting;

namespace HomeBudget.Application.Reporting.GetBudgetBalanceHistory;

/// <summary>
/// Represents a query that gets historical budget balances.
/// </summary>
public sealed record GetBudgetBalanceHistoryQuery(Guid OwnerId, int? Year = null, int? Limit = null)
    : IQuery<BudgetBalanceListResponse>;
