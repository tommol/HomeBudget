using HomeBudget.Application.Execution.AddExpense;
using HomeBudget.Application.Tests.Execution;
using HomeBudget.Application.Tests.Planning;
using HomeBudget.Domain.Shared;
using static HomeBudget.Application.Tests.Execution.ExecutionTestData;

namespace HomeBudget.Application.Tests.Execution.AddExpense;

public sealed class AddExpenseCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_AddsExpense()
    {
        var budget = CreateBudgetAggregate();
        var category = CreateExpenseCategory(budget.OwnerId);
        var budgetRepository = new FakeBudgetRepository(budget);
        var categoryRepository = new FakeBudgetCategoryRepository(category);
        var handler = new AddExpenseCommandHandler(budgetRepository, categoryRepository);

        var expenseId = await handler.HandleAsync(new AddExpenseCommand(
            budget.OwnerId.Value,
            budget.Id.Value,
            category.Id.Value,
            "Groceries",
            250m,
            "pln",
            new DateOnly(2026, 7, 12)));

        var expense = Assert.Single(budget.Expenses);
        Assert.Equal(expense.Id.Value, expenseId);
        Assert.Equal(category.Id, expense.CategoryId);
        Assert.Equal("Groceries", expense.Title);
        Assert.Equal(new Money(250m, Currency.PLN), expense.Amount);
        Assert.Equal(new Money(250m, Currency.PLN), budget.TotalExpenses);
        Assert.Contains(budget, budgetRepository.UpdatedBudgets);
    }
}
