using Asp.Versioning;
using HomeBudget.Api.Auth;
using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Planning.CloseBudgetPlan;

namespace HomeBudget.Api.Endpoints.Planning;

internal static class CloseBudgetPlanEndpoint
{
    public static IEndpointRouteBuilder MapCloseBudgetPlanEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{budgetPlanId:guid}/close", HandleAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("CloseBudgetPlan")
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
        ICommandHandler<CloseBudgetPlanCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await PlanningEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new CloseBudgetPlanCommand(currentOwner.OwnerId, budgetPlanId);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }
}
