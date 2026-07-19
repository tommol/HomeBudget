using HomeBudget.Application.Execution.CreateBudgetFromApprovedPlan;
using HomeBudget.Application.Tests.Execution;
using HomeBudget.Application.Tests.Planning;
using HomeBudget.Domain.Execution;
using HomeBudget.Domain.Planning;
using static HomeBudget.Application.Tests.Planning.PlanningTestData;

namespace HomeBudget.Application.Tests.Execution.CreateBudgetFromApprovedPlan;

public sealed class CreateBudgetFromApprovedPlanHandlerTests
{
    [Fact]
    public async Task HandleAsync_CreatesBudget_WhenPlanBecomesActive()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var budgetRepository = new FakeBudgetRepository();
        var handler = new CreateBudgetFromApprovedPlanHandler(budgetPlanRepository, budgetRepository);

        await handler.HandleAsync(new BudgetPlanStatusChangedEvent(
            budgetPlan.Id,
            BudgetPlanStatus.Draft,
            BudgetPlanStatus.Active,
            DateTimeOffset.UtcNow));

        var budget = Assert.Single(budgetRepository.AddedBudgets);
        Assert.Equal(new BudgetId(budgetPlan.Id.Value), budget.Id);
        Assert.Equal(budgetPlan.Id, budget.SourceBudgetPlanId);
        Assert.Equal(budgetPlan.OwnerId, budget.OwnerId);
        Assert.Equal(budgetPlan.Period, budget.Period);
        Assert.Equal(budgetPlan.DefaultCurrency, budget.DefaultCurrency);
        Assert.Equal(BudgetStatus.Active, budget.Status);
        Assert.Contains(budget, budgetRepository.Budgets);
    }

    [Fact]
    public async Task HandleAsync_DoesNothing_WhenPlanDoesNotBecomeActive()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var budgetRepository = new FakeBudgetRepository();
        var handler = new CreateBudgetFromApprovedPlanHandler(budgetPlanRepository, budgetRepository);

        await handler.HandleAsync(new BudgetPlanStatusChangedEvent(
            budgetPlan.Id,
            BudgetPlanStatus.Active,
            BudgetPlanStatus.Closed,
            DateTimeOffset.UtcNow));

        Assert.Empty(budgetRepository.AddedBudgets);
    }

    [Fact]
    public async Task HandleAsync_DoesNothing_WhenBudgetAlreadyExists()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var existingBudget = ExecutionTestData.CreateBudgetAggregate(
            ownerId: budgetPlan.OwnerId,
            budgetId: new BudgetId(budgetPlan.Id.Value),
            sourceBudgetPlanId: budgetPlan.Id);
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var budgetRepository = new FakeBudgetRepository(existingBudget);
        var handler = new CreateBudgetFromApprovedPlanHandler(budgetPlanRepository, budgetRepository);

        await handler.HandleAsync(new BudgetPlanStatusChangedEvent(
            budgetPlan.Id,
            BudgetPlanStatus.Draft,
            BudgetPlanStatus.Active,
            DateTimeOffset.UtcNow));

        Assert.Empty(budgetRepository.AddedBudgets);
        Assert.Single(budgetRepository.Budgets);
    }
}
