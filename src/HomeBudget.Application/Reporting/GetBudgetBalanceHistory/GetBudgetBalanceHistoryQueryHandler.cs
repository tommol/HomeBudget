using HomeBudget.Application.Abstractions;
using HomeBudget.Contracts.Reporting;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Reporting.GetBudgetBalanceHistory;

/// <summary>
/// Handles queries that get historical budget balances.
/// </summary>
public sealed class GetBudgetBalanceHistoryQueryHandler
    : IQueryHandler<GetBudgetBalanceHistoryQuery, BudgetBalanceListResponse>
{
    private const int DefaultLimit = 12;

    private readonly IBudgetBalanceReadRepository _budgetBalanceReadRepository;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBudgetBalanceHistoryQueryHandler"/> class.
    /// </summary>
    /// <param name="budgetBalanceReadRepository">The budget balance read repository.</param>
    /// <param name="timeProvider">The server time provider.</param>
    public GetBudgetBalanceHistoryQueryHandler(
        IBudgetBalanceReadRepository budgetBalanceReadRepository,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(budgetBalanceReadRepository);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _budgetBalanceReadRepository = budgetBalanceReadRepository;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<BudgetBalanceListResponse> HandleAsync(
        GetBudgetBalanceHistoryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var limit = query.Limit ?? DefaultLimit;
        if (limit <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(GetBudgetBalanceHistoryQuery.Limit),
                "History limit must be greater than zero.");
        }

        var ownerId = new OwnerId(query.OwnerId);
        var now = _timeProvider.GetLocalNow();
        var currentPeriod = new BudgetPeriod(now.Year, now.Month);

        var balances = await _budgetBalanceReadRepository
            .GetHistoryAsync(ownerId, currentPeriod, query.Year, limit, cancellationToken)
            .ConfigureAwait(false);

        return new BudgetBalanceListResponse(balances);
    }
}
