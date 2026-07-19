using HomeBudget.Application.Planning;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Tests.Planning;

internal static class PlanningTestData
{
    public static BudgetPlan CreateBudgetPlanAggregate(
        BudgetPeriod? period = null,
        OwnerId? ownerId = null,
        Currency? defaultCurrency = null)
        => new(
            new BudgetPlanId(Guid.NewGuid()),
            ownerId ?? new OwnerId(Guid.NewGuid()),
            period ?? new BudgetPeriod(2026, 7),
            defaultCurrency ?? Currency.PLN);

    public static BudgetCategory CreateExpenseCategory(OwnerId ownerId)
        => CreateCategory(ownerId, BudgetCategoryType.Expense);

    public static BudgetCategory CreateIncomeCategory(OwnerId ownerId)
        => CreateCategory(ownerId, BudgetCategoryType.Income);

    public static BudgetCategory CreateSavingCategory(OwnerId ownerId)
        => CreateCategory(ownerId, BudgetCategoryType.Saving);

    public static BudgetCategory CreateCategory(OwnerId ownerId, BudgetCategoryType type)
        => new(
            new BudgetCategoryId(Guid.NewGuid()),
            ownerId,
            "Category",
            type);
}

internal sealed class FakeBudgetPlanRepository : IBudgetPlanRepository
{
    private readonly List<BudgetPlan> _budgetPlans = [];

    public FakeBudgetPlanRepository(params BudgetPlan[] budgetPlans)
    {
        _budgetPlans.AddRange(budgetPlans);
    }

    public List<BudgetPlan> BudgetPlans => _budgetPlans;
    public List<BudgetPlan> AddedBudgetPlans { get; } = [];
    public List<BudgetPlan> UpdatedBudgetPlans { get; } = [];
    public List<CancellationToken> GetByIdCancellationTokens { get; } = [];
    public List<CancellationToken> AddCancellationTokens { get; } = [];
    public List<CancellationToken> UpdateCancellationTokens { get; } = [];

    public Task<BudgetPlan?> GetByIdAsync(BudgetPlanId id, CancellationToken cancellationToken = default)
    {
        GetByIdCancellationTokens.Add(cancellationToken);

        return Task.FromResult<BudgetPlan?>(_budgetPlans.SingleOrDefault(budgetPlan => budgetPlan.Id.Equals(id)));
    }

    public Task<BudgetPlan?> GetByIdAndOwnerIdAsync(
        BudgetPlanId id,
        OwnerId ownerId,
        CancellationToken cancellationToken = default)
    {
        GetByIdCancellationTokens.Add(cancellationToken);

        return Task.FromResult<BudgetPlan?>(_budgetPlans.SingleOrDefault(
            budgetPlan => budgetPlan.Id.Equals(id)
                && budgetPlan.OwnerId.Equals(ownerId)));
    }

    public Task<bool> ExistsByOwnerIdAndPeriodAsync(
        OwnerId ownerId,
        BudgetPeriod period,
        CancellationToken cancellationToken = default)
    {
        GetByIdCancellationTokens.Add(cancellationToken);

        return Task.FromResult(_budgetPlans.Any(
            budgetPlan => budgetPlan.OwnerId.Equals(ownerId)
                && budgetPlan.Period.Equals(period)));
    }

    public Task AddAsync(BudgetPlan budgetPlan, CancellationToken cancellationToken = default)
    {
        _budgetPlans.Add(budgetPlan);
        AddedBudgetPlans.Add(budgetPlan);
        AddCancellationTokens.Add(cancellationToken);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(BudgetPlan budgetPlan, CancellationToken cancellationToken = default)
    {
        UpdatedBudgetPlans.Add(budgetPlan);
        UpdateCancellationTokens.Add(cancellationToken);

        return Task.CompletedTask;
    }
}

internal sealed class FakeBudgetCategoryRepository : IBudgetCategoryRepository
{
    private readonly List<BudgetCategory> _categories = [];

    public FakeBudgetCategoryRepository(params BudgetCategory[] categories)
    {
        _categories.AddRange(categories);
    }

    public List<CancellationToken> GetByIdCancellationTokens { get; } = [];

    public Task<BudgetCategory?> GetByIdAsync(BudgetCategoryId id, CancellationToken cancellationToken = default)
    {
        GetByIdCancellationTokens.Add(cancellationToken);

        return Task.FromResult<BudgetCategory?>(_categories.SingleOrDefault(category => category.Id.Equals(id)));
    }

    public Task<BudgetCategory?> GetByIdAndOwnerIdAsync(
        BudgetCategoryId id,
        OwnerId ownerId,
        CancellationToken cancellationToken = default)
    {
        GetByIdCancellationTokens.Add(cancellationToken);

        return Task.FromResult<BudgetCategory?>(_categories.SingleOrDefault(
            category => category.Id.Equals(id)
                && category.OwnerId.Equals(ownerId)));
    }
}
