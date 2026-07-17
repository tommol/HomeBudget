using HomeBudget.Application.Execution.AddSaving;
using HomeBudget.Application.Tests.Execution;
using HomeBudget.Application.Tests.Planning;
using HomeBudget.Domain.Shared;
using static HomeBudget.Application.Tests.Execution.ExecutionTestData;

namespace HomeBudget.Application.Tests.Execution.AddSaving;

public sealed class AddSavingCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_AddsSaving()
    {
        var budget = CreateBudgetAggregate();
        var category = CreateSavingCategory(budget.OwnerId);
        var budgetRepository = new FakeBudgetRepository(budget);
        var categoryRepository = new FakeBudgetCategoryRepository(category);
        var handler = new AddSavingCommandHandler(budgetRepository, categoryRepository);

        var savingId = await handler.HandleAsync(new AddSavingCommand(
            budget.OwnerId.Value,
            budget.Id.Value,
            category.Id.Value,
            "Emergency fund",
            1000m,
            "pln",
            new DateOnly(2026, 7, 15)));

        var saving = Assert.Single(budget.Savings);
        Assert.Equal(saving.Id.Value, savingId);
        Assert.Equal(category.Id, saving.CategoryId);
        Assert.Equal("Emergency fund", saving.Title);
        Assert.Equal(new Money(1000m, Currency.PLN), saving.Amount);
        Assert.Equal(new Money(1000m, Currency.PLN), budget.TotalSavings);
        Assert.Contains(budget, budgetRepository.UpdatedBudgets);
    }
}
