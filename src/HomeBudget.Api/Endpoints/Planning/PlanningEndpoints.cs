using HomeBudget.Api.Auth;

namespace HomeBudget.Api.Endpoints.Planning;

internal static class PlanningEndpoints
{
    public static IEndpointRouteBuilder MapPlanningEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/planning/budget-plans")
            .WithTags("Planning / Budget Plans")
            .AddEndpointFilter<CurrentOwnerEndpointFilter>();

        group.MapCreateBudgetPlanEndpoint();
        group.MapCopyBudgetPlanEndpoint();
        group.MapAddPlannedIncomeEndpoint();
        group.MapAddExpenseCategoryAllocationEndpoint();
        group.MapChangeExpenseCategoryAllocationAmountEndpoint();
        group.MapChangeExpenseCategoryAllocationFlexibilityEndpoint();
        group.MapRemoveExpenseCategoryAllocationEndpoint();
        group.MapAddSavingContributionEndpoint();
        group.MapChangeSavingContributionAmountEndpoint();
        group.MapRemoveSavingContributionEndpoint();
        group.MapActivateBudgetPlanEndpoint();
        group.MapCloseBudgetPlanEndpoint();

        return app;
    }
}
