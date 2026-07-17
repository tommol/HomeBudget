using Asp.Versioning;
using HomeBudget.Api.Auth;
using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Planning.CopyBudgetPlan;
using HomeBudget.Contracts.Planning;

namespace HomeBudget.Api.Endpoints.Planning;

internal static class CopyBudgetPlanEndpoint
{
    public static IEndpointRouteBuilder MapCopyBudgetPlanEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{sourceBudgetPlanId:guid}/copies", HandleAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("CopyBudgetPlan")
            .Produces<CopyBudgetPlanResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid sourceBudgetPlanId,
        CopyBudgetPlanRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<CopyBudgetPlanCommand, Guid> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await PlanningEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new CopyBudgetPlanCommand(
                currentOwner.OwnerId,
                sourceBudgetPlanId,
                request.Year,
                request.Month,
                request.CopyPlannedIncomes,
                request.CopyExpenseCategoryAllocations,
                request.CopySavingContributions);

            var budgetPlanId = await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.Created(
                $"/api/v1/planning/budget-plans/{budgetPlanId}",
                new CopyBudgetPlanResponse(budgetPlanId));
        });
    }
}
