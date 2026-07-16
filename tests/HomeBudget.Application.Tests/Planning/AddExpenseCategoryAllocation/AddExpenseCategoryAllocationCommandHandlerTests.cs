using HomeBudget.Application.Planning;
using HomeBudget.Application.Planning.AddExpenseCategoryAllocation;
using HomeBudget.Application.Tests.Planning;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;
using static HomeBudget.Application.Tests.Planning.PlanningTestData;

namespace HomeBudget.Application.Tests.Planning.AddExpenseCategoryAllocation;

public sealed class AddExpenseCategoryAllocationCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_AddsAllocation()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var category = CreateExpenseCategory(budgetPlan.OwnerId);
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var categoryRepository = new FakeBudgetCategoryRepository(category);
        var handler = new AddExpenseCategoryAllocationCommandHandler(budgetPlanRepository, categoryRepository);

        var allocationId = await handler.HandleAsync(new AddExpenseCategoryAllocationCommand(
            budgetPlan.Id.Value,
            category.Id.Value,
            3000m,
            "fixed"));

        var allocation = Assert.Single(budgetPlan.ExpenseCategoryAllocations);
        Assert.Equal(allocation.Id.Value, allocationId);
        Assert.Equal(category.Id, allocation.CategoryId);
        Assert.Equal(new Money(3000m, Currency.PLN), allocation.Amount);
        Assert.Equal(CategoryAllocationFlexibility.Fixed, allocation.Flexibility);
        Assert.Contains(budgetPlan, budgetPlanRepository.UpdatedBudgetPlans);
    }

    [Fact]
    public async Task HandleAsync_Throws_WhenCategoryDoesNotExist()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var missingCategoryId = Guid.NewGuid();
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var categoryRepository = new FakeBudgetCategoryRepository();
        var handler = new AddExpenseCategoryAllocationCommandHandler(budgetPlanRepository, categoryRepository);

        var exception = await Assert.ThrowsAsync<BudgetCategoryNotFoundException>(() => handler.HandleAsync(
            new AddExpenseCategoryAllocationCommand(
                budgetPlan.Id.Value,
                missingCategoryId,
                3000m,
                "fixed")));

        Assert.Equal(missingCategoryId, exception.BudgetCategoryId);
        Assert.Empty(budgetPlan.ExpenseCategoryAllocations);
        Assert.Empty(budgetPlanRepository.UpdatedBudgetPlans);
    }

    [Fact]
    public async Task HandleAsync_Throws_WhenFlexibilityIsInvalid()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var category = CreateExpenseCategory(budgetPlan.OwnerId);
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var categoryRepository = new FakeBudgetCategoryRepository(category);
        var handler = new AddExpenseCategoryAllocationCommandHandler(budgetPlanRepository, categoryRepository);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(
            new AddExpenseCategoryAllocationCommand(
                budgetPlan.Id.Value,
                category.Id.Value,
                3000m,
                "sometimes")));

        Assert.Equal("Flexibility", exception.ParamName);
        Assert.Empty(budgetPlan.ExpenseCategoryAllocations);
        Assert.Empty(budgetPlanRepository.UpdatedBudgetPlans);
    }
}
