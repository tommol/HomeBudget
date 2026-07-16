using HomeBudget.Application.Planning;
using HomeBudget.Application.Planning.AddSavingContribution;
using HomeBudget.Application.Tests.Planning;
using HomeBudget.Domain.Shared;
using static HomeBudget.Application.Tests.Planning.PlanningTestData;

namespace HomeBudget.Application.Tests.Planning.AddSavingContribution;

public sealed class AddSavingContributionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_AddsContribution()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var category = CreateSavingCategory(budgetPlan.OwnerId);
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var categoryRepository = new FakeBudgetCategoryRepository(category);
        var handler = new AddSavingContributionCommandHandler(budgetPlanRepository, categoryRepository);

        var contributionId = await handler.HandleAsync(new AddSavingContributionCommand(
            budgetPlan.Id.Value,
            category.Id.Value,
            500m));

        var contribution = Assert.Single(budgetPlan.SavingContributions);
        Assert.Equal(contribution.Id.Value, contributionId);
        Assert.Equal(category.Id, contribution.CategoryId);
        Assert.Equal(new Money(500m, Currency.PLN), contribution.Amount);
        Assert.Contains(budgetPlan, budgetPlanRepository.UpdatedBudgetPlans);
    }

    [Fact]
    public async Task HandleAsync_Throws_WhenCategoryDoesNotExist()
    {
        var budgetPlan = CreateBudgetPlanAggregate();
        var missingCategoryId = Guid.NewGuid();
        var budgetPlanRepository = new FakeBudgetPlanRepository(budgetPlan);
        var categoryRepository = new FakeBudgetCategoryRepository();
        var handler = new AddSavingContributionCommandHandler(budgetPlanRepository, categoryRepository);

        var exception = await Assert.ThrowsAsync<BudgetCategoryNotFoundException>(() => handler.HandleAsync(
            new AddSavingContributionCommand(
                budgetPlan.Id.Value,
                missingCategoryId,
                500m)));

        Assert.Equal(missingCategoryId, exception.BudgetCategoryId);
        Assert.Empty(budgetPlan.SavingContributions);
        Assert.Empty(budgetPlanRepository.UpdatedBudgetPlans);
    }
}
