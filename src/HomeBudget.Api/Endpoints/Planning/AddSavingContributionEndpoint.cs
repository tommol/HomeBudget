using Asp.Versioning;
using HomeBudget.Api.Auth;
using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Planning.AddSavingContribution;
using HomeBudget.Contracts.Planning;

namespace HomeBudget.Api.Endpoints.Planning;

internal static class AddSavingContributionEndpoint
{
    public static IEndpointRouteBuilder MapAddSavingContributionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{budgetPlanId:guid}/saving-contributions", HandleAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("AddSavingContribution")
            .Produces<AddSavingContributionResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid budgetPlanId,
        AddSavingContributionRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<AddSavingContributionCommand, Guid> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await PlanningEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new AddSavingContributionCommand(
                currentOwner.OwnerId,
                budgetPlanId,
                request.CategoryId,
                request.Amount);

            var savingContributionId = await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.Created(
                $"/api/v1/planning/budget-plans/{budgetPlanId}/saving-contributions/{savingContributionId}",
                new AddSavingContributionResponse(savingContributionId));
        });
    }
}
