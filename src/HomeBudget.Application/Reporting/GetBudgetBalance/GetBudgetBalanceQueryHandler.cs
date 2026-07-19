using HomeBudget.Application.Abstractions;
using HomeBudget.Contracts.Reporting;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Reporting.GetBudgetBalance;

/// <summary>
/// Handles queries that get a budget balance for a specific period.
/// </summary>
public sealed class GetBudgetBalanceQueryHandler : IQueryHandler<GetBudgetBalanceQuery, BudgetBalanceResponse>
{
    private readonly IBudgetBalanceReadRepository _budgetBalanceReadRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBudgetBalanceQueryHandler"/> class.
    /// </summary>
    /// <param name="budgetBalanceReadRepository">The budget balance read repository.</param>
    public GetBudgetBalanceQueryHandler(IBudgetBalanceReadRepository budgetBalanceReadRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetBalanceReadRepository);

        _budgetBalanceReadRepository = budgetBalanceReadRepository;
    }

    /// <inheritdoc />
    public async Task<BudgetBalanceResponse> HandleAsync(
        GetBudgetBalanceQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var ownerId = new OwnerId(query.OwnerId);
        var period = new BudgetPeriod(query.Year, query.Month);

        return await _budgetBalanceReadRepository
            .GetByOwnerIdAndPeriodAsync(ownerId, period, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new BudgetBalanceNotFoundException(query.Year, query.Month);
    }
}
