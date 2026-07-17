using Asp.Versioning;
using HomeBudget.Api.Auth;
using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Planning.CreateBudgetPlan;
using HomeBudget.Contracts.Planning;

namespace HomeBudget.Api.Endpoints;

internal static class PlanningEndpoints
{
    public static IEndpointRouteBuilder MapPlanningEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/planning/budget-plans")
            .WithTags("Planning / Budget Plans");

        group.MapPost("/", CreateBudgetPlanAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .AddEndpointFilter<CurrentOwnerEndpointFilter>()
            .WithName("CreateBudgetPlan")
            .Produces<CreateBudgetPlanResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    private static async Task<IResult> CreateBudgetPlanAsync(
        CreateBudgetPlanRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<CreateBudgetPlanCommand, Guid> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        try
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
        }
        catch (ArgumentException exception)
        {
            return TypedResults.Problem(
                title: "Invalid request.",
                detail: exception.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
