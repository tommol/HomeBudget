using HomeBudget.Application.Planning;
using HomeBudget.Application.Planning.AddPlannedIncome;
using HomeBudget.Application.Tests.Planning;
using HomeBudget.Domain.Shared;
using static HomeBudget.Application.Tests.Planning.PlanningTestData;

namespace HomeBudget.Application.Tests.Planning.AddPlannedIncome;

public sealed class AddPlannedIncomeCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_AddsPlannedIncome()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var category = CreateIncomeCategory(budgetPlan.OwnerId);
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var categoryRepository = new FakeBudgetCategoryRepository(category);
        var handler = new AddPlannedIncomeCommandHandler(budgetPlanRepository, categoryRepository);

        var plannedIncomeId = await handler.HandleAsync(new AddPlannedIncomeCommand(
            budgetPlan.Id.Value,
            category.Id.Value,
            "Salary",
            5000m,
            "pln",
            new DateOnly(2026, 7, 10)));

        var plannedIncome = Assert.Single(budgetPlan.PlannedIncomes);
        Assert.Equal(plannedIncome.Id.Value, plannedIncomeId);
        Assert.Equal(category.Id, plannedIncome.CategoryId);
        Assert.Equal("Salary", plannedIncome.Title);
        Assert.Equal(new Money(5000m, Currency.PLN), plannedIncome.Amount);
        Assert.Contains(budgetPlan, budgetPlanRepository.UpdatedBudgetPlans);
    }

    [Fact]
    public async Task HandleAsync_AddsConvertedAmount()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var category = CreateIncomeCategory(budgetPlan.OwnerId);
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var categoryRepository = new FakeBudgetCategoryRepository(category);
        var handler = new AddPlannedIncomeCommandHandler(budgetPlanRepository, categoryRepository);
        var conversionDate = new DateOnly(2026, 7, 9);

        await handler.HandleAsync(new AddPlannedIncomeCommand(
            budgetPlan.Id.Value,
            category.Id.Value,
            "Bonus",
            1000m,
            "EUR",
            new DateOnly(2026, 7, 10),
            ConvertedAmount: 4250m,
            ConversionDate: conversionDate));

        var plannedIncome = Assert.Single(budgetPlan.PlannedIncomes);
        Assert.Equal(new Money(1000m, Currency.EUR), plannedIncome.Amount);
        Assert.Equal(new Money(4250m, Currency.PLN), plannedIncome.ConvertedAmount);
        Assert.Equal(conversionDate, plannedIncome.ConversionDate);
        Assert.Equal(new Money(4250m, Currency.PLN), budgetPlan.TotalPlannedIncome);
        Assert.Contains(budgetPlan, budgetPlanRepository.UpdatedBudgetPlans);
    }

    [Fact]
    public async Task HandleAsync_Throws_WhenCategoryDoesNotExist()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var missingCategoryId = Guid.NewGuid();
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var categoryRepository = new FakeBudgetCategoryRepository();
        var handler = new AddPlannedIncomeCommandHandler(budgetPlanRepository, categoryRepository);

        var exception = await Assert.ThrowsAsync<BudgetCategoryNotFoundException>(() => handler.HandleAsync(
            new AddPlannedIncomeCommand(
                budgetPlan.Id.Value,
                missingCategoryId,
                "Salary",
                5000m,
                "PLN",
                new DateOnly(2026, 7, 10))));

        Assert.Equal(missingCategoryId, exception.BudgetCategoryId);
        Assert.Empty(budgetPlan.PlannedIncomes);
        Assert.Empty(budgetPlanRepository.UpdatedBudgetPlans);
    }
}
