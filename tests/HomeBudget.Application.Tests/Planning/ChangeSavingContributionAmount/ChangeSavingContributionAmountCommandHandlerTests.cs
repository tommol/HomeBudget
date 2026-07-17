using HomeBudget.Application.Planning.ChangeSavingContributionAmount;
using HomeBudget.Application.Tests.Planning;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;
using static HomeBudget.Application.Tests.Planning.PlanningTestData;

namespace HomeBudget.Application.Tests.Planning.ChangeSavingContributionAmount;

public sealed class ChangeSavingContributionAmountCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ChangesAmount()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var contribution = budgetPlan.AddSavingContribution(
            new SavingContributionId(Guid.NewGuid()),
            CreateSavingCategory(budgetPlan.OwnerId),
            new Money(500m, Currency.PLN));
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var handler = new ChangeSavingContributionAmountCommandHandler(budgetPlanRepository);

        await handler.HandleAsync(new ChangeSavingContributionAmountCommand(
            budgetPlan.OwnerId.Value,
            budgetPlan.Id.Value,
            contribution.Id.Value,
            750m));

        Assert.Equal(new Money(750m, Currency.PLN), contribution.Amount);
        Assert.Equal(new Money(750m, Currency.PLN), budgetPlan.TotalSavingContributions);
        Assert.Contains(budgetPlan, budgetPlanRepository.UpdatedBudgetPlans);
    }
}
