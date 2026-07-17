using Asp.Versioning;
using HomeBudget.Api.Auth;
using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Planning.AddPlannedIncome;
using HomeBudget.Contracts.Planning;

namespace HomeBudget.Api.Endpoints.Planning;

internal static class AddPlannedIncomeEndpoint
{
    public static IEndpointRouteBuilder MapAddPlannedIncomeEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{budgetPlanId:guid}/planned-incomes", HandleAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("AddPlannedIncome")
            .Produces<AddPlannedIncomeResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid budgetPlanId,
        AddPlannedIncomeRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<AddPlannedIncomeCommand, Guid> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await PlanningEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new AddPlannedIncomeCommand(
                currentOwner.OwnerId,
                budgetPlanId,
                request.CategoryId,
                request.Title,
                request.Amount,
                request.CurrencyCode,
                request.ExpectedDate,
                request.ConvertedAmount,
                request.ConversionDate);

            var plannedIncomeId = await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.Created(
                $"/api/v1/planning/budget-plans/{budgetPlanId}/planned-incomes/{plannedIncomeId}",
                new AddPlannedIncomeResponse(plannedIncomeId));
        });
    }
}
