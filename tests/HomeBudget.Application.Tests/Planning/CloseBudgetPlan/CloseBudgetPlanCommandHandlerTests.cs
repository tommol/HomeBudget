using HomeBudget.Application.Planning.CloseBudgetPlan;
using HomeBudget.Application.Tests.Planning;
using HomeBudget.Domain.Planning;
using static HomeBudget.Application.Tests.Planning.PlanningTestData;

namespace HomeBudget.Application.Tests.Planning.CloseBudgetPlan;

public sealed class CloseBudgetPlanCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ClosesPlan()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var handler = new CloseBudgetPlanCommandHandler(budgetPlanRepository);

        await handler.HandleAsync(new CloseBudgetPlanCommand(budgetPlan.OwnerId.Value, budgetPlan.Id.Value));

        Assert.Equal(BudgetPlanStatus.Closed, budgetPlan.Status);
        Assert.Contains(budgetPlan, budgetPlanRepository.UpdatedBudgetPlans);
    }
}
