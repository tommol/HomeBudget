using HomeBudget.Application.Reporting;
using HomeBudget.Contracts.Reporting;
using HomeBudget.Domain.Execution;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace HomeBudget.Infrastructure.Server.Persistence.Repositories;

internal sealed class EfBudgetBalanceReadRepository : IBudgetBalanceReadRepository
{
    private const string PeriodYearPropertyName = "_periodYear";
    private const string PeriodMonthPropertyName = "_periodMonth";

    private readonly HomeBudgetDbContext _dbContext;

    public EfBudgetBalanceReadRepository(HomeBudgetDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public async Task<BudgetBalanceResponse?> GetByOwnerIdAndPeriodAsync(
        OwnerId ownerId,
        BudgetPeriod period,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ownerId);
        ArgumentNullException.ThrowIfNull(period);

        var budgetPlans = _dbContext.BudgetPlans
            .AsNoTracking()
            .Where(budgetPlan => budgetPlan.OwnerId == ownerId
                && EF.Property<int>(budgetPlan, PeriodYearPropertyName) == period.Year
                && EF.Property<int>(budgetPlan, PeriodMonthPropertyName) == period.Month);

        var projection = await ProjectBalances(budgetPlans)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return projection is null ? null : Map(projection);
    }

    public async Task<IReadOnlyCollection<BudgetBalanceResponse>> GetHistoryAsync(
        OwnerId ownerId,
        BudgetPeriod currentPeriod,
        int? year,
        int limit,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ownerId);
        ArgumentNullException.ThrowIfNull(currentPeriod);

        var budgetPlans = _dbContext.BudgetPlans
            .AsNoTracking()
            .Where(budgetPlan => budgetPlan.OwnerId == ownerId)
            .Where(budgetPlan => EF.Property<int>(budgetPlan, PeriodYearPropertyName) < currentPeriod.Year
                || (EF.Property<int>(budgetPlan, PeriodYearPropertyName) == currentPeriod.Year
                    && EF.Property<int>(budgetPlan, PeriodMonthPropertyName) < currentPeriod.Month));

        if (year is not null)
        {
            budgetPlans = budgetPlans.Where(
                budgetPlan => EF.Property<int>(budgetPlan, PeriodYearPropertyName) == year.Value);
        }

        budgetPlans = budgetPlans
            .OrderByDescending(budgetPlan => EF.Property<int>(budgetPlan, PeriodYearPropertyName))
            .ThenByDescending(budgetPlan => EF.Property<int>(budgetPlan, PeriodMonthPropertyName))
            .Take(limit);

        var projections = await ProjectBalances(budgetPlans)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return projections.Select(Map).ToArray();
    }

    private IQueryable<BudgetBalanceProjection> ProjectBalances(IQueryable<BudgetPlan> budgetPlans)
        => from budgetPlan in budgetPlans
           join budget in _dbContext.Budgets.AsNoTracking()
               on budgetPlan.Id equals budget.SourceBudgetPlanId into budgets
           from budget in budgets.DefaultIfEmpty()
           select new BudgetBalanceProjection(
               EF.Property<int>(budgetPlan, PeriodYearPropertyName),
               EF.Property<int>(budgetPlan, PeriodMonthPropertyName),
               budgetPlan.Id,
               budget == null ? null : budget.Id,
               budgetPlan.DefaultCurrency,
               budgetPlan.Status,
               budget == null ? null : (BudgetStatus?)budget.Status,
               budgetPlan.TotalPlannedIncome.Amount,
               budget == null ? 0m : budget.TotalIncome.Amount,
               budgetPlan.TotalAllocatedExpenses.Amount,
               budget == null ? 0m : budget.TotalExpenses.Amount,
               budgetPlan.TotalSavingContributions.Amount,
               budget == null ? 0m : budget.TotalSavings.Amount,
               budgetPlan.PlannedFinancialResult.Amount,
               budget == null ? 0m : budget.ActualFinancialResult.Amount);

    private static BudgetBalanceResponse Map(BudgetBalanceProjection projection)
        => new(
            projection.Year,
            projection.Month,
            projection.BudgetPlanId.Value,
            projection.BudgetId?.Value,
            projection.Currency.Code,
            projection.BudgetPlanStatus.ToString(),
            projection.BudgetStatus?.ToString(),
            projection.PlannedIncome,
            projection.ActualIncome,
            projection.ActualIncome - projection.PlannedIncome,
            projection.PlannedExpenses,
            projection.ActualExpenses,
            projection.ActualExpenses - projection.PlannedExpenses,
            projection.PlannedSavings,
            projection.ActualSavings,
            projection.ActualSavings - projection.PlannedSavings,
            projection.PlannedResult,
            projection.ActualResult,
            projection.ActualResult - projection.PlannedResult);

    private sealed record BudgetBalanceProjection(
        int Year,
        int Month,
        BudgetPlanId BudgetPlanId,
        BudgetId? BudgetId,
        Currency Currency,
        BudgetPlanStatus BudgetPlanStatus,
        BudgetStatus? BudgetStatus,
        decimal PlannedIncome,
        decimal ActualIncome,
        decimal PlannedExpenses,
        decimal ActualExpenses,
        decimal PlannedSavings,
        decimal ActualSavings,
        decimal PlannedResult,
        decimal ActualResult);
}
