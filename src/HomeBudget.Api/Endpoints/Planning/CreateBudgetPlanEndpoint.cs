using Asp.Versioning;
using HomeBudget.Api.Auth;
using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Planning.CreateBudgetPlan;
using HomeBudget.Contracts.Planning;

namespace HomeBudget.Api.Endpoints.Planning;

internal static class CreateBudgetPlanEndpoint
{
    public static IEndpointRouteBuilder MapCreateBudgetPlanEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", HandleAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("CreateBudgetPlan")
            .Produces<CreateBudgetPlanResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        CreateBudgetPlanRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<CreateBudgetPlanCommand, Guid> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await PlanningEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new CreateBudgetPlanCommand(
                currentOwner.OwnerId,
                request.Year,
                request.Month,
                request.DefaultCurrencyCode);

            var budgetPlanId = await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.Created(
                $"/api/v1/planning/budget-plans/{budgetPlanId}",
                new CreateBudgetPlanResponse(budgetPlanId));
        });
    }
}
