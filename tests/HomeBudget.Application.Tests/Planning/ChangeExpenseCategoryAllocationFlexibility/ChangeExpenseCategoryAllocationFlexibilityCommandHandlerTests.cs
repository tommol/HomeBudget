using HomeBudget.Application.Planning.ChangeExpenseCategoryAllocationFlexibility;
using HomeBudget.Application.Tests.Planning;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;
using static HomeBudget.Application.Tests.Planning.PlanningTestData;

namespace HomeBudget.Application.Tests.Planning.ChangeExpenseCategoryAllocationFlexibility;

public sealed class ChangeExpenseCategoryAllocationFlexibilityCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ChangesFlexibility()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var allocation = budgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            CreateExpenseCategory(budgetPlan.OwnerId),
            new Money(3000m, Currency.PLN),
            CategoryAllocationFlexibility.Fixed);
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var handler = new ChangeExpenseCategoryAllocationFlexibilityCommandHandler(budgetPlanRepository);

        await handler.HandleAsync(new ChangeExpenseCategoryAllocationFlexibilityCommand(
            budgetPlan.OwnerId.Value,
            budgetPlan.Id.Value,
            allocation.Id.Value,
            "optional"));

        Assert.Equal(CategoryAllocationFlexibility.Optional, allocation.Flexibility);
        Assert.Contains(budgetPlan, budgetPlanRepository.UpdatedBudgetPlans);
    }

    [Fact]
    public async Task HandleAsync_Throws_WhenFlexibilityIsInvalid()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var allocation = budgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            CreateExpenseCategory(budgetPlan.OwnerId),
            new Money(3000m, Currency.PLN),
            CategoryAllocationFlexibility.Fixed);
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var handler = new ChangeExpenseCategoryAllocationFlexibilityCommandHandler(budgetPlanRepository);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(
            new ChangeExpenseCategoryAllocationFlexibilityCommand(
                budgetPlan.OwnerId.Value,
                budgetPlan.Id.Value,
                allocation.Id.Value,
                "sometimes")));

        Assert.Equal("Flexibility", exception.ParamName);
        Assert.Equal(CategoryAllocationFlexibility.Fixed, allocation.Flexibility);
        Assert.Empty(budgetPlanRepository.UpdatedBudgetPlans);
    }
}
