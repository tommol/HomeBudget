using Asp.Versioning;
using HomeBudget.Api.Auth;
using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Planning.RemoveExpenseCategoryAllocation;

namespace HomeBudget.Api.Endpoints.Planning;

internal static class RemoveExpenseCategoryAllocationEndpoint
{
    public static IEndpointRouteBuilder MapRemoveExpenseCategoryAllocationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/{budgetPlanId:guid}/expense-category-allocations/{categoryAllocationId:guid}",
                HandleAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("RemoveExpenseCategoryAllocation")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid budgetPlanId,
        Guid categoryAllocationId,
        ICurrentOwner currentOwner,
        ICommandHandler<RemoveExpenseCategoryAllocationCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await PlanningEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new RemoveExpenseCategoryAllocationCommand(
                currentOwner.OwnerId,
                budgetPlanId,
                categoryAllocationId);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }
}
