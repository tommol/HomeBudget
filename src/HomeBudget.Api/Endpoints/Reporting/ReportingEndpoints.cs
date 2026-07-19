using Asp.Versioning;
using HomeBudget.Api.Auth;
using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Reporting.GetBudgetBalance;
using HomeBudget.Application.Reporting.GetBudgetBalanceHistory;
using HomeBudget.Application.Reporting.GetCurrentBudgetBalance;
using HomeBudget.Contracts.Reporting;

namespace HomeBudget.Api.Endpoints.Reporting;

internal static class ReportingEndpoints
{
    public static IEndpointRouteBuilder MapReportingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/reporting/budget-balances")
            .WithTags("Reporting / Budget Balances")
            .AddEndpointFilter<CurrentOwnerEndpointFilter>();

        group.MapGet("/current", GetCurrentAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("GetCurrentBudgetBalance")
            .Produces<BudgetBalanceResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/history", GetHistoryAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("GetBudgetBalanceHistory")
            .Produces<BudgetBalanceListResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{year:int}/{month:int}", GetByPeriodAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("GetBudgetBalanceByPeriod")
            .Produces<BudgetBalanceResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    private static async Task<IResult> GetCurrentAsync(
        ICurrentOwner currentOwner,
        IQueryHandler<GetCurrentBudgetBalanceQuery, BudgetBalanceResponse> handler,
        CancellationToken cancellationToken)
    {
        return await ReportingEndpointExecutor.ExecuteAsync(async () =>
        {
            var query = new GetCurrentBudgetBalanceQuery(currentOwner.OwnerId);
            var balance = await handler.HandleAsync(query, cancellationToken);

            return TypedResults.Ok(balance);
        });
    }

    private static async Task<IResult> GetHistoryAsync(
        int? year,
        int? limit,
        ICurrentOwner currentOwner,
        IQueryHandler<GetBudgetBalanceHistoryQuery, BudgetBalanceListResponse> handler,
        CancellationToken cancellationToken)
    {
        return await ReportingEndpointExecutor.ExecuteAsync(async () =>
        {
            var query = new GetBudgetBalanceHistoryQuery(currentOwner.OwnerId, year, limit);
            var balances = await handler.HandleAsync(query, cancellationToken);

            return TypedResults.Ok(balances);
        });
    }

    private static async Task<IResult> GetByPeriodAsync(
        int year,
        int month,
        ICurrentOwner currentOwner,
        IQueryHandler<GetBudgetBalanceQuery, BudgetBalanceResponse> handler,
        CancellationToken cancellationToken)
    {
        return await ReportingEndpointExecutor.ExecuteAsync(async () =>
        {
            var query = new GetBudgetBalanceQuery(currentOwner.OwnerId, year, month);
            var balance = await handler.HandleAsync(query, cancellationToken);

            return TypedResults.Ok(balance);
        });
    }
}
