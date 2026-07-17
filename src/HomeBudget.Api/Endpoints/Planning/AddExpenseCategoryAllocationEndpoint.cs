using Asp.Versioning;
using HomeBudget.Api.Auth;
using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Planning.AddExpenseCategoryAllocation;
using HomeBudget.Contracts.Planning;

namespace HomeBudget.Api.Endpoints.Planning;

internal static class AddExpenseCategoryAllocationEndpoint
{
    public static IEndpointRouteBuilder MapAddExpenseCategoryAllocationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{budgetPlanId:guid}/expense-category-allocations", HandleAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("AddExpenseCategoryAllocation")
            .Produces<AddExpenseCategoryAllocationResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid budgetPlanId,
        AddExpenseCategoryAllocationRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<AddExpenseCategoryAllocationCommand, Guid> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await PlanningEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new AddExpenseCategoryAllocationCommand(
                currentOwner.OwnerId,
                budgetPlanId,
                request.CategoryId,
                request.Amount,
                request.Flexibility);

            var categoryAllocationId = await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.Created(
                $"/api/v1/planning/budget-plans/{budgetPlanId}/expense-category-allocations/{categoryAllocationId}",
                new AddExpenseCategoryAllocationResponse(categoryAllocationId));
        });
    }
}
