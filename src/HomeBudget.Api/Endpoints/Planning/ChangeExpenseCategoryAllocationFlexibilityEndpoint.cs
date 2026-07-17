using Asp.Versioning;
using HomeBudget.Api.Auth;
using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Planning.ChangeExpenseCategoryAllocationFlexibility;
using HomeBudget.Contracts.Planning;

namespace HomeBudget.Api.Endpoints.Planning;

internal static class ChangeExpenseCategoryAllocationFlexibilityEndpoint
{
    public static IEndpointRouteBuilder MapChangeExpenseCategoryAllocationFlexibilityEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPatch(
                "/{budgetPlanId:guid}/expense-category-allocations/{categoryAllocationId:guid}/flexibility",
                HandleAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("ChangeExpenseCategoryAllocationFlexibility")
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
        ChangeExpenseCategoryAllocationFlexibilityRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<ChangeExpenseCategoryAllocationFlexibilityCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await PlanningEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new ChangeExpenseCategoryAllocationFlexibilityCommand(
                currentOwner.OwnerId,
                budgetPlanId,
                categoryAllocationId,
                request.Flexibility);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }
}
