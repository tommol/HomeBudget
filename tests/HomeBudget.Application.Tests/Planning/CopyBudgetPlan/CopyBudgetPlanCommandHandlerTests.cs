using HomeBudget.Application.Planning;
using HomeBudget.Application.Planning.CopyBudgetPlan;
using HomeBudget.Application.Tests.Planning;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;
using static HomeBudget.Application.Tests.Planning.PlanningTestData;

namespace HomeBudget.Application.Tests.Planning.CopyBudgetPlan;

public sealed class CopyBudgetPlanCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_CopiesBudgetPlanToTargetPeriod()
    {
        var sourceBudgetPlan = CreateBudgetPlanAggregate(new BudgetPeriod(2026, 1));
        var income = sourceBudgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            CreateIncomeCategory(sourceBudgetPlan.OwnerId),
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 1, 31));
        var allocation = sourceBudgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            CreateExpenseCategory(sourceBudgetPlan.OwnerId),
            new Money(3000m, Currency.PLN),
            CategoryAllocationFlexibility.Fixed);
        var contribution = sourceBudgetPlan.AddSavingContribution(
            new SavingContributionId(Guid.NewGuid()),
            CreateSavingCategory(sourceBudgetPlan.OwnerId),
            new Money(500m, Currency.PLN));
        var repository = new FakeBudgetPlanRepository(sourceBudgetPlan);
        var handler = new CopyBudgetPlanCommandHandler(repository);

        var copiedBudgetPlanId = await handler.HandleAsync(
            new CopyBudgetPlanCommand(sourceBudgetPlan.OwnerId.Value, sourceBudgetPlan.Id.Value, 2026, 2));

        var copiedBudgetPlan = Assert.Single(repository.AddedBudgetPlans);
        Assert.Equal(copiedBudgetPlan.Id.Value, copiedBudgetPlanId);
        Assert.NotEqual(sourceBudgetPlan.Id.Value, copiedBudgetPlanId);
        Assert.Equal(sourceBudgetPlan.OwnerId, copiedBudgetPlan.OwnerId);
        Assert.Equal(new BudgetPeriod(2026, 2), copiedBudgetPlan.Period);
        Assert.Equal(sourceBudgetPlan.DefaultCurrency, copiedBudgetPlan.DefaultCurrency);
        Assert.Equal(BudgetPlanStatus.Draft, copiedBudgetPlan.Status);

        var copiedIncome = Assert.Single(copiedBudgetPlan.PlannedIncomes);
        Assert.NotEqual(income.Id, copiedIncome.Id);
        Assert.Equal(income.CategoryId, copiedIncome.CategoryId);
        Assert.Equal(new DateOnly(2026, 2, 28), copiedIncome.ExpectedDate);

        var copiedAllocation = Assert.Single(copiedBudgetPlan.ExpenseCategoryAllocations);
        Assert.NotEqual(allocation.Id, copiedAllocation.Id);
        Assert.Equal(allocation.CategoryId, copiedAllocation.CategoryId);

        var copiedContribution = Assert.Single(copiedBudgetPlan.SavingContributions);
        Assert.NotEqual(contribution.Id, copiedContribution.Id);
        Assert.Equal(contribution.CategoryId, copiedContribution.CategoryId);
    }

    [Fact]
    public async Task HandleAsync_PassesCopyFlags()
    {
        var sourceBudgetPlan = CreateBudgetPlanAggregate(new BudgetPeriod(2026, 7));
        sourceBudgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            CreateIncomeCategory(sourceBudgetPlan.OwnerId),
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10));
        sourceBudgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            CreateExpenseCategory(sourceBudgetPlan.OwnerId),
            new Money(3000m, Currency.PLN),
            CategoryAllocationFlexibility.Fixed);
        sourceBudgetPlan.AddSavingContribution(
            new SavingContributionId(Guid.NewGuid()),
            CreateSavingCategory(sourceBudgetPlan.OwnerId),
            new Money(500m, Currency.PLN));
        var repository = new FakeBudgetPlanRepository(sourceBudgetPlan);
        var handler = new CopyBudgetPlanCommandHandler(repository);

        await handler.HandleAsync(new CopyBudgetPlanCommand(
            sourceBudgetPlan.OwnerId.Value,
            sourceBudgetPlan.Id.Value,
            2026,
            8,
            CopyPlannedIncomes: false,
            CopyExpenseCategoryAllocations: false));

        var copiedBudgetPlan = Assert.Single(repository.AddedBudgetPlans);
        Assert.Empty(copiedBudgetPlan.PlannedIncomes);
        Assert.Empty(copiedBudgetPlan.ExpenseCategoryAllocations);
        Assert.Single(copiedBudgetPlan.SavingContributions);
    }

    [Fact]
    public async Task HandleAsync_Throws_WhenSourceBudgetPlanDoesNotExist()
    {
        var sourceBudgetPlanId = Guid.NewGuid();
        var repository = new FakeBudgetPlanRepository();
        var handler = new CopyBudgetPlanCommandHandler(repository);

        var exception = await Assert.ThrowsAsync<BudgetPlanNotFoundException>(() => handler.HandleAsync(
            new CopyBudgetPlanCommand(Guid.NewGuid(), sourceBudgetPlanId, 2026, 8)));

        Assert.Equal(sourceBudgetPlanId, exception.BudgetPlanId);
        Assert.Empty(repository.AddedBudgetPlans);
    }
}
