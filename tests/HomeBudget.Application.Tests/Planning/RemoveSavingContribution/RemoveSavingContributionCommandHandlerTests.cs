using HomeBudget.Application.Planning.RemoveSavingContribution;
using HomeBudget.Application.Tests.Planning;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;
using static HomeBudget.Application.Tests.Planning.PlanningTestData;

namespace HomeBudget.Application.Tests.Planning.RemoveSavingContribution;

public sealed class RemoveSavingContributionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_RemovesContribution()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var contribution = budgetPlan.AddSavingContribution(
            new SavingContributionId(Guid.NewGuid()),
            CreateSavingCategory(budgetPlan.OwnerId),
            new Money(500m, Currency.PLN));
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var handler = new RemoveSavingContributionCommandHandler(budgetPlanRepository);

        await handler.HandleAsync(new RemoveSavingContributionCommand(
            budgetPlan.OwnerId.Value,
            budgetPlan.Id.Value,
            contribution.Id.Value));

        Assert.Empty(budgetPlan.SavingContributions);
        Assert.Equal(Money.Zero(Currency.PLN), budgetPlan.TotalSavingContributions);
        Assert.Contains(budgetPlan, budgetPlanRepository.UpdatedBudgetPlans);
    }
}
