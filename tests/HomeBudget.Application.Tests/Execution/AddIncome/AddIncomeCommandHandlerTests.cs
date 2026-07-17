using HomeBudget.Application.Execution.AddIncome;
using HomeBudget.Application.Planning;
using HomeBudget.Application.Tests.Execution;
using HomeBudget.Application.Tests.Planning;
using HomeBudget.Domain.Shared;
using static HomeBudget.Application.Tests.Execution.ExecutionTestData;

namespace HomeBudget.Application.Tests.Execution.AddIncome;

public sealed class AddIncomeCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_AddsIncome()
    {
        var budget = CreateBudgetAggregate();
        var category = CreateIncomeCategory(budget.OwnerId);
        var budgetRepository = new FakeBudgetRepository(budget);
        var categoryRepository = new FakeBudgetCategoryRepository(category);
        var handler = new AddIncomeCommandHandler(budgetRepository, categoryRepository);

        var incomeId = await handler.HandleAsync(new AddIncomeCommand(
            budget.OwnerId.Value,
            budget.Id.Value,
            category.Id.Value,
            "Salary",
            5000m,
            "pln",
            new DateOnly(2026, 7, 10)));

        var income = Assert.Single(budget.Incomes);
        Assert.Equal(income.Id.Value, incomeId);
        Assert.Equal(category.Id, income.CategoryId);
        Assert.Equal("Salary", income.Title);
        Assert.Equal(new Money(5000m, Currency.PLN), income.Amount);
        Assert.Equal(new Money(5000m, Currency.PLN), budget.TotalIncome);
        Assert.Contains(budget, budgetRepository.UpdatedBudgets);
    }

    [Fact]
    public async Task HandleAsync_AddsConvertedAmount()
    {
        var budget = CreateBudgetAggregate();
        var category = CreateIncomeCategory(budget.OwnerId);
        var budgetRepository = new FakeBudgetRepository(budget);
        var categoryRepository = new FakeBudgetCategoryRepository(category);
        var handler = new AddIncomeCommandHandler(budgetRepository, categoryRepository);
        var conversionDate = new DateOnly(2026, 7, 9);

        await handler.HandleAsync(new AddIncomeCommand(
            budget.OwnerId.Value,
            budget.Id.Value,
            category.Id.Value,
            "Bonus",
            1000m,
            "EUR",
            new DateOnly(2026, 7, 10),
            ConvertedAmount: 4250m,
            ConversionDate: conversionDate));

        var income = Assert.Single(budget.Incomes);
        Assert.Equal(new Money(1000m, Currency.EUR), income.Amount);
        Assert.Equal(new Money(4250m, Currency.PLN), income.ConvertedAmount);
        Assert.Equal(conversionDate, income.ConversionDate);
        Assert.Equal(new Money(4250m, Currency.PLN), budget.TotalIncome);
        Assert.Contains(budget, budgetRepository.UpdatedBudgets);
    }

    [Fact]
    public async Task HandleAsync_Throws_WhenCategoryDoesNotExist()
    {
        var budget = CreateBudgetAggregate();
        var missingCategoryId = Guid.NewGuid();
        var budgetRepository = new FakeBudgetRepository(budget);
        var categoryRepository = new FakeBudgetCategoryRepository();
        var handler = new AddIncomeCommandHandler(budgetRepository, categoryRepository);

        var exception = await Assert.ThrowsAsync<BudgetCategoryNotFoundException>(() => handler.HandleAsync(
            new AddIncomeCommand(
                budget.OwnerId.Value,
                budget.Id.Value,
                missingCategoryId,
                "Salary",
                5000m,
                "PLN",
                new DateOnly(2026, 7, 10))));

        Assert.Equal(missingCategoryId, exception.BudgetCategoryId);
        Assert.Empty(budget.Incomes);
        Assert.Empty(budgetRepository.UpdatedBudgets);
    }
}
