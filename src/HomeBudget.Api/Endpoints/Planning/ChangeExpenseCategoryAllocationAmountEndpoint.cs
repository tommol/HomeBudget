using Asp.Versioning;
using HomeBudget.Api.Auth;
using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Planning.ChangeExpenseCategoryAllocationAmount;
using HomeBudget.Contracts.Planning;

namespace HomeBudget.Api.Endpoints.Planning;

internal static class ChangeExpenseCategoryAllocationAmountEndpoint
{
    public static IEndpointRouteBuilder MapChangeExpenseCategoryAllocationAmountEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPatch(
                "/{budgetPlanId:guid}/expense-category-allocations/{categoryAllocationId:guid}/amount",
                HandleAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("ChangeExpenseCategoryAllocationAmount")
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
        ChangeExpenseCategoryAllocationAmountRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<ChangeExpenseCategoryAllocationAmountCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await PlanningEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new ChangeExpenseCategoryAllocationAmountCommand(
                currentOwner.OwnerId,
                budgetPlanId,
                categoryAllocationId,
                request.Amount);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }
}
