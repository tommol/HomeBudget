using HomeBudget.Application.Execution;
using HomeBudget.Domain.Execution;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Tests.Execution;

internal static class ExecutionTestData
{
    public static Budget CreateBudgetAggregate(
        BudgetPeriod? period = null,
        OwnerId? ownerId = null,
        Currency? defaultCurrency = null,
        BudgetId? budgetId = null,
        BudgetPlanId? sourceBudgetPlanId = null)
        => new(
            budgetId ?? new BudgetId(Guid.NewGuid()),
            ownerId ?? new OwnerId(Guid.NewGuid()),
            period ?? new BudgetPeriod(2026, 7),
            defaultCurrency ?? Currency.PLN,
            sourceBudgetPlanId ?? new BudgetPlanId(Guid.NewGuid()));

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

internal sealed class FakeBudgetRepository : IBudgetRepository
{
    private readonly List<Budget> _budgets = [];

    public FakeBudgetRepository(params Budget[] budgets)
    {
        _budgets.AddRange(budgets);
    }

    public List<Budget> Budgets => _budgets;
    public List<Budget> AddedBudgets { get; } = [];
    public List<Budget> UpdatedBudgets { get; } = [];
    public List<CancellationToken> GetByIdCancellationTokens { get; } = [];
    public List<CancellationToken> AddCancellationTokens { get; } = [];
    public List<CancellationToken> UpdateCancellationTokens { get; } = [];

    public Task<Budget?> GetByIdAsync(BudgetId id, CancellationToken cancellationToken = default)
    {
        GetByIdCancellationTokens.Add(cancellationToken);

        return Task.FromResult<Budget?>(_budgets.SingleOrDefault(budget => budget.Id.Equals(id)));
    }

    public Task<Budget?> GetByIdAndOwnerIdAsync(
        BudgetId id,
        OwnerId ownerId,
        CancellationToken cancellationToken = default)
    {
        GetByIdCancellationTokens.Add(cancellationToken);

        return Task.FromResult<Budget?>(_budgets.SingleOrDefault(
            budget => budget.Id.Equals(id)
                && budget.OwnerId.Equals(ownerId)));
    }

    public Task AddAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        _budgets.Add(budget);
        AddedBudgets.Add(budget);
        AddCancellationTokens.Add(cancellationToken);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        UpdatedBudgets.Add(budget);
        UpdateCancellationTokens.Add(cancellationToken);

        return Task.CompletedTask;
    }
}
