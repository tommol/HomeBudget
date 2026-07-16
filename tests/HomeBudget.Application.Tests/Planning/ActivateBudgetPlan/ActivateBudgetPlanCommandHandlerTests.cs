using HomeBudget.Application.Planning;
using HomeBudget.Application.Planning.ActivateBudgetPlan;
using HomeBudget.Application.Tests.Planning;
using HomeBudget.Domain.Planning;
using static HomeBudget.Application.Tests.Planning.PlanningTestData;

namespace HomeBudget.Application.Tests.Planning.ActivateBudgetPlan;

public sealed class ActivateBudgetPlanCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ActivatesPlan()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var handler = new ActivateBudgetPlanCommandHandler(budgetPlanRepository);

        await handler.HandleAsync(new ActivateBudgetPlanCommand(budgetPlan.Id.Value));

        Assert.Equal(BudgetPlanStatus.Active, budgetPlan.Status);
        Assert.Contains(budgetPlan, budgetPlanRepository.UpdatedBudgetPlans);
    }

    [Fact]
    public async Task HandleAsync_Throws_WhenBudgetPlanDoesNotExist()
    {
        var missingBudgetPlanId = Guid.NewGuid();
        var budgetPlanRepository = new FakeBudgetPlanRepository();
        var handler = new ActivateBudgetPlanCommandHandler(budgetPlanRepository);

        var exception = await Assert.ThrowsAsync<BudgetPlanNotFoundException>(() => handler.HandleAsync(
            new ActivateBudgetPlanCommand(missingBudgetPlanId)));

        Assert.Equal(missingBudgetPlanId, exception.BudgetPlanId);
        Assert.Empty(budgetPlanRepository.UpdatedBudgetPlans);
    }
}
