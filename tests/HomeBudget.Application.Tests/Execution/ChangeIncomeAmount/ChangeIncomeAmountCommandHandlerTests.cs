using HomeBudget.Application.Execution.ChangeIncomeAmount;
using HomeBudget.Application.Tests.Execution;
using HomeBudget.Domain.Execution;
using HomeBudget.Domain.Shared;
using static HomeBudget.Application.Tests.Execution.ExecutionTestData;

namespace HomeBudget.Application.Tests.Execution.ChangeIncomeAmount;

public sealed class ChangeIncomeAmountCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ChangesIncomeAmount()
    {
        var budget = CreateBudgetAggregate();
        var income = budget.AddIncome(
            new IncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budget.OwnerId),
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10));
        var budgetRepository = new FakeBudgetRepository(budget);
        var handler = new ChangeIncomeAmountCommandHandler(budgetRepository);

        await handler.HandleAsync(new ChangeIncomeAmountCommand(
            budget.OwnerId.Value,
            budget.Id.Value,
            income.Id.Value,
            5500m,
            "pln"));

        Assert.Equal(new Money(5500m, Currency.PLN), income.Amount);
        Assert.Equal(new Money(5500m, Currency.PLN), budget.TotalIncome);
        Assert.Contains(budget, budgetRepository.UpdatedBudgets);
    }
}
