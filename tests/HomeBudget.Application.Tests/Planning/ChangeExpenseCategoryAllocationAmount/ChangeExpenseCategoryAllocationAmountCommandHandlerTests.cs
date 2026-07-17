using HomeBudget.Application.Planning.ChangeExpenseCategoryAllocationAmount;
using HomeBudget.Application.Tests.Planning;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;
using static HomeBudget.Application.Tests.Planning.PlanningTestData;

namespace HomeBudget.Application.Tests.Planning.ChangeExpenseCategoryAllocationAmount;

public sealed class ChangeExpenseCategoryAllocationAmountCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ChangesAmount()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var allocation = budgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            CreateExpenseCategory(budgetPlan.OwnerId),
            new Money(3000m, Currency.PLN),
            CategoryAllocationFlexibility.Fixed);
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var handler = new ChangeExpenseCategoryAllocationAmountCommandHandler(budgetPlanRepository);

        await handler.HandleAsync(new ChangeExpenseCategoryAllocationAmountCommand(
            budgetPlan.OwnerId.Value,
            budgetPlan.Id.Value,
            allocation.Id.Value,
            3500m));

        Assert.Equal(new Money(3500m, Currency.PLN), allocation.Amount);
        Assert.Equal(new Money(3500m, Currency.PLN), budgetPlan.TotalAllocatedExpenses);
        Assert.Contains(budgetPlan, budgetPlanRepository.UpdatedBudgetPlans);
    }

    [Fact]
    public async Task HandleAsync_PassesCancellationTokenToRepository()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var allocation = budgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            CreateExpenseCategory(budgetPlan.OwnerId),
            new Money(3000m, Currency.PLN),
            CategoryAllocationFlexibility.Fixed);
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var handler = new ChangeExpenseCategoryAllocationAmountCommandHandler(budgetPlanRepository);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        await handler.HandleAsync(
            new ChangeExpenseCategoryAllocationAmountCommand(
                budgetPlan.OwnerId.Value,
                budgetPlan.Id.Value,
                allocation.Id.Value,
                3500m),
            cancellationToken);

        Assert.Equal(cancellationToken, Assert.Single(budgetPlanRepository.GetByIdCancellationTokens));
        Assert.Equal(cancellationToken, Assert.Single(budgetPlanRepository.UpdateCancellationTokens));
    }
}
