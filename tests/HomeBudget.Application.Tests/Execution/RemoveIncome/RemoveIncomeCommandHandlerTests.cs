using HomeBudget.Application.Execution.RemoveIncome;
using HomeBudget.Application.Tests.Execution;
using HomeBudget.Domain.Execution;
using HomeBudget.Domain.Shared;
using static HomeBudget.Application.Tests.Execution.ExecutionTestData;

namespace HomeBudget.Application.Tests.Execution.RemoveIncome;

public sealed class RemoveIncomeCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_RemovesIncome()
    {
        var budget = CreateBudgetAggregate();
        var income = budget.AddIncome(
            new IncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budget.OwnerId),
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10));
        var budgetRepository = new FakeBudgetRepository(budget);
        var handler = new RemoveIncomeCommandHandler(budgetRepository);

        await handler.HandleAsync(new RemoveIncomeCommand(
            budget.OwnerId.Value,
            budget.Id.Value,
            income.Id.Value,
            "Duplicate"));

        Assert.True(income.IsRemoved);
        Assert.Equal("Duplicate", income.RemovalReason);
        Assert.Equal(Money.Zero(Currency.PLN), budget.TotalIncome);
        Assert.Contains(budget, budgetRepository.UpdatedBudgets);
    }
}
