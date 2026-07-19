using HomeBudget.Application.Abstractions;
using HomeBudget.Contracts.Reporting;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Reporting.GetCurrentBudgetBalance;

/// <summary>
/// Handles queries that get the current budget balance.
/// </summary>
public sealed class GetCurrentBudgetBalanceQueryHandler : IQueryHandler<GetCurrentBudgetBalanceQuery, BudgetBalanceResponse>
{
    private readonly IBudgetBalanceReadRepository _budgetBalanceReadRepository;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrentBudgetBalanceQueryHandler"/> class.
    /// </summary>
    /// <param name="budgetBalanceReadRepository">The budget balance read repository.</param>
    /// <param name="timeProvider">The server time provider.</param>
    public GetCurrentBudgetBalanceQueryHandler(
        IBudgetBalanceReadRepository budgetBalanceReadRepository,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(budgetBalanceReadRepository);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _budgetBalanceReadRepository = budgetBalanceReadRepository;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<BudgetBalanceResponse> HandleAsync(
        GetCurrentBudgetBalanceQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var ownerId = new OwnerId(query.OwnerId);
        var now = _timeProvider.GetLocalNow();
        var period = new BudgetPeriod(now.Year, now.Month);

        return await _budgetBalanceReadRepository
            .GetByOwnerIdAndPeriodAsync(ownerId, period, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new BudgetBalanceNotFoundException(period.Year, period.Month);
    }
}
