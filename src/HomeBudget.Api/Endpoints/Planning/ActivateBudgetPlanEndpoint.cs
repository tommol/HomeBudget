using Asp.Versioning;
using HomeBudget.Api.Auth;
using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Planning.ActivateBudgetPlan;

namespace HomeBudget.Api.Endpoints.Planning;

internal static class ActivateBudgetPlanEndpoint
{
    public static IEndpointRouteBuilder MapActivateBudgetPlanEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{budgetPlanId:guid}/activate", HandleAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("ActivateBudgetPlan")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid budgetPlanId,
        ICurrentOwner currentOwner,
        ICommandHandler<ActivateBudgetPlanCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await PlanningEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new ActivateBudgetPlanCommand(currentOwner.OwnerId, budgetPlanId);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }
}
