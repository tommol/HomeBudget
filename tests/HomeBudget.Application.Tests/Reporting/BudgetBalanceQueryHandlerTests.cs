using HomeBudget.Application.Reporting;
using HomeBudget.Application.Reporting.GetBudgetBalance;
using HomeBudget.Application.Reporting.GetBudgetBalanceHistory;
using HomeBudget.Application.Reporting.GetCurrentBudgetBalance;
using HomeBudget.Contracts.Reporting;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Tests.Reporting;

public sealed class BudgetBalanceQueryHandlerTests
{
    [Fact]
    public async Task GetBudgetBalance_ReturnsBalanceForPeriod()
    {
        var ownerId = Guid.NewGuid();
        var balance = CreateBalance(year: 2026, month: 7);
        var repository = new FakeBudgetBalanceReadRepository(balance);
        var handler = new GetBudgetBalanceQueryHandler(repository);

        var result = await handler.HandleAsync(new GetBudgetBalanceQuery(ownerId, 2026, 7));

        Assert.Equal(balance, result);
        Assert.Equal(new OwnerId(ownerId), repository.RequestedOwnerIds.Single());
        Assert.Equal(new BudgetPeriod(2026, 7), repository.RequestedPeriods.Single());
    }

    [Fact]
    public async Task GetBudgetBalance_Throws_WhenBalanceDoesNotExist()
    {
        var repository = new FakeBudgetBalanceReadRepository();
        var handler = new GetBudgetBalanceQueryHandler(repository);

        var exception = await Assert.ThrowsAsync<BudgetBalanceNotFoundException>(
            () => handler.HandleAsync(new GetBudgetBalanceQuery(Guid.NewGuid(), 2026, 7)));

        Assert.Equal(2026, exception.Year);
        Assert.Equal(7, exception.Month);
    }

    [Fact]
    public async Task GetCurrentBudgetBalance_UsesServerCurrentPeriod()
    {
        var ownerId = Guid.NewGuid();
        var balance = CreateBalance(year: 2026, month: 8);
        var repository = new FakeBudgetBalanceReadRepository(balance);
        var handler = new GetCurrentBudgetBalanceQueryHandler(
            repository,
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 15, 12, 0, 0, TimeSpan.Zero)));

        var result = await handler.HandleAsync(new GetCurrentBudgetBalanceQuery(ownerId));

        Assert.Equal(balance, result);
        Assert.Equal(new BudgetPeriod(2026, 8), repository.RequestedPeriods.Single());
    }

    [Fact]
    public async Task GetBudgetBalanceHistory_UsesCurrentPeriodAndDefaultLimit()
    {
        var ownerId = Guid.NewGuid();
        var balances = new[] { CreateBalance(year: 2026, month: 6) };
        var repository = new FakeBudgetBalanceReadRepository(history: balances);
        var handler = new GetBudgetBalanceHistoryQueryHandler(
            repository,
            new FixedTimeProvider(new DateTimeOffset(2026, 7, 19, 12, 0, 0, TimeSpan.Zero)));

        var result = await handler.HandleAsync(new GetBudgetBalanceHistoryQuery(ownerId));

        Assert.Equal(balances, result.Items);
        Assert.Equal(new OwnerId(ownerId), repository.HistoryOwnerIds.Single());
        Assert.Equal(new BudgetPeriod(2026, 7), repository.HistoryCurrentPeriods.Single());
        Assert.Null(repository.HistoryYears.Single());
        Assert.Equal(12, repository.HistoryLimits.Single());
    }

    [Fact]
    public async Task GetBudgetBalanceHistory_RejectsNonPositiveLimit()
    {
        var repository = new FakeBudgetBalanceReadRepository();
        var handler = new GetBudgetBalanceHistoryQueryHandler(
            repository,
            new FixedTimeProvider(new DateTimeOffset(2026, 7, 19, 12, 0, 0, TimeSpan.Zero)));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => handler.HandleAsync(new GetBudgetBalanceHistoryQuery(Guid.NewGuid(), Limit: 0)));
    }

    private static BudgetBalanceResponse CreateBalance(int year, int month)
        => new(
            year,
            month,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "PLN",
            "Active",
            "Active",
            5000m,
            5200m,
            200m,
            3000m,
            2900m,
            -100m,
            500m,
            600m,
            100m,
            1500m,
            1700m,
            200m);

    private sealed class FakeBudgetBalanceReadRepository : IBudgetBalanceReadRepository
    {
        private readonly BudgetBalanceResponse? _balance;
        private readonly IReadOnlyCollection<BudgetBalanceResponse> _history;

        public FakeBudgetBalanceReadRepository(
            BudgetBalanceResponse? balance = null,
            IReadOnlyCollection<BudgetBalanceResponse>? history = null)
        {
            _balance = balance;
            _history = history ?? [];
        }

        public List<OwnerId> RequestedOwnerIds { get; } = [];
        public List<BudgetPeriod> RequestedPeriods { get; } = [];
        public List<OwnerId> HistoryOwnerIds { get; } = [];
        public List<BudgetPeriod> HistoryCurrentPeriods { get; } = [];
        public List<int?> HistoryYears { get; } = [];
        public List<int> HistoryLimits { get; } = [];

        public Task<BudgetBalanceResponse?> GetByOwnerIdAndPeriodAsync(
            OwnerId ownerId,
            BudgetPeriod period,
            CancellationToken cancellationToken = default)
        {
            RequestedOwnerIds.Add(ownerId);
            RequestedPeriods.Add(period);

            return Task.FromResult(_balance);
        }

        public Task<IReadOnlyCollection<BudgetBalanceResponse>> GetHistoryAsync(
            OwnerId ownerId,
            BudgetPeriod currentPeriod,
            int? year,
            int limit,
            CancellationToken cancellationToken = default)
        {
            HistoryOwnerIds.Add(ownerId);
            HistoryCurrentPeriods.Add(currentPeriod);
            HistoryYears.Add(year);
            HistoryLimits.Add(limit);

            return Task.FromResult(_history);
        }
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;

        public FixedTimeProvider(DateTimeOffset now)
        {
            _now = now;
        }

        public override DateTimeOffset GetUtcNow() => _now.ToUniversalTime();

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }
}
