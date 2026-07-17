using HomeBudget.Application.Planning.RemoveExpenseCategoryAllocation;
using HomeBudget.Application.Tests.Planning;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;
using static HomeBudget.Application.Tests.Planning.PlanningTestData;

namespace HomeBudget.Application.Tests.Planning.RemoveExpenseCategoryAllocation;

public sealed class RemoveExpenseCategoryAllocationCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_RemovesAllocation()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var allocation = budgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            CreateExpenseCategory(budgetPlan.OwnerId),
            new Money(3000m, Currency.PLN),
            CategoryAllocationFlexibility.Optional);
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var handler = new RemoveExpenseCategoryAllocationCommandHandler(budgetPlanRepository);

        await handler.HandleAsync(new RemoveExpenseCategoryAllocationCommand(
            budgetPlan.OwnerId.Value,
            budgetPlan.Id.Value,
            allocation.Id.Value));

        Assert.Empty(budgetPlan.ExpenseCategoryAllocations);
        Assert.Equal(Money.Zero(Currency.PLN), budgetPlan.TotalAllocatedExpenses);
        Assert.Contains(budgetPlan, budgetPlanRepository.UpdatedBudgetPlans);
    }

    [Fact]
    public async Task HandleAsync_Throws_WhenAllocationIsFixed()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var allocation = budgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            CreateExpenseCategory(budgetPlan.OwnerId),
            new Money(3000m, Currency.PLN),
            CategoryAllocationFlexibility.Fixed);
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var handler = new RemoveExpenseCategoryAllocationCommandHandler(budgetPlanRepository);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(
            new RemoveExpenseCategoryAllocationCommand(
                budgetPlan.OwnerId.Value,
                budgetPlan.Id.Value,
                allocation.Id.Value)));

        Assert.Contains(allocation, budgetPlan.ExpenseCategoryAllocations);
        Assert.Empty(budgetPlanRepository.UpdatedBudgetPlans);
    }
}
