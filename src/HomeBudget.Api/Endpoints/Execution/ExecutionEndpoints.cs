using HomeBudget.Api.Auth;

namespace HomeBudget.Api.Endpoints.Execution;

internal static class ExecutionEndpoints
{
    public static IEndpointRouteBuilder MapExecutionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/execution/budgets")
            .WithTags("Execution / Budgets")
            .AddEndpointFilter<CurrentOwnerEndpointFilter>();

        group.MapIncomeEndpoints();
        group.MapExpenseEndpoints();
        group.MapSavingEndpoints();
        group.MapCloseBudgetEndpoint();

        return app;
    }
}
