using HomeBudget.Application.Planning;
using HomeBudget.Application.Planning.CreateBudgetPlan;
using HomeBudget.Application.Tests.Planning;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Tests.Planning.CreateBudgetPlan;

public sealed class CreateBudgetPlanCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_CreatesBudgetPlan()
    {
        var repository = new FakeBudgetPlanRepository();
        var handler = new CreateBudgetPlanCommandHandler(repository);
        var ownerId = Guid.NewGuid();
        var command = new CreateBudgetPlanCommand(ownerId, 2026, 7, "pln");

        var budgetPlanId = await handler.HandleAsync(command);

        var budgetPlan = Assert.Single(repository.BudgetPlans);
        Assert.Equal(budgetPlan.Id.Value, budgetPlanId);
        Assert.Equal(new OwnerId(ownerId), budgetPlan.OwnerId);
        Assert.Equal(new BudgetPeriod(2026, 7), budgetPlan.Period);
        Assert.Equal(Currency.PLN, budgetPlan.DefaultCurrency);
        Assert.Equal(BudgetPlanStatus.Draft, budgetPlan.Status);
    }

    [Fact]
    public async Task HandleAsync_PassesCancellationToken()
    {
        var repository = new FakeBudgetPlanRepository();
        var handler = new CreateBudgetPlanCommandHandler(repository);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        await handler.HandleAsync(
            new CreateBudgetPlanCommand(Guid.NewGuid(), 2026, 7, "PLN"),
            cancellationToken);

        Assert.Equal(cancellationToken, Assert.Single(repository.AddCancellationTokens));
    }
}
