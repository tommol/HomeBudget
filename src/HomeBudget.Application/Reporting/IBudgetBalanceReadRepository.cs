using HomeBudget.Contracts.Reporting;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Reporting;

/// <summary>
/// Provides read operations for budget balance projections.
/// </summary>
public interface IBudgetBalanceReadRepository
{
    /// <summary>
    /// Gets a budget balance by owner and period.
    /// </summary>
    /// <param name="ownerId">The owner identifier.</param>
    /// <param name="period">The budget period.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching budget balance, or <c>null</c> when no plan exists for the period.</returns>
    Task<BudgetBalanceResponse?> GetByOwnerIdAndPeriodAsync(
        OwnerId ownerId,
        BudgetPeriod period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets historical budget balances before the current period.
    /// </summary>
    /// <param name="ownerId">The owner identifier.</param>
    /// <param name="currentPeriod">The current budget period.</param>
    /// <param name="year">An optional year filter.</param>
    /// <param name="limit">The maximum number of balances to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching historical budget balances.</returns>
    Task<IReadOnlyCollection<BudgetBalanceResponse>> GetHistoryAsync(
        OwnerId ownerId,
        BudgetPeriod currentPeriod,
        int? year,
        int limit,
        CancellationToken cancellationToken = default);
}
