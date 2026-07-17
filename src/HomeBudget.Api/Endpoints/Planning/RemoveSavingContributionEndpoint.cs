using Asp.Versioning;
using HomeBudget.Api.Auth;
using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Planning.RemoveSavingContribution;

namespace HomeBudget.Api.Endpoints.Planning;

internal static class RemoveSavingContributionEndpoint
{
    public static IEndpointRouteBuilder MapRemoveSavingContributionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/{budgetPlanId:guid}/saving-contributions/{savingContributionId:guid}",
                HandleAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("RemoveSavingContribution")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid budgetPlanId,
        Guid savingContributionId,
        ICurrentOwner currentOwner,
        ICommandHandler<RemoveSavingContributionCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await PlanningEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new RemoveSavingContributionCommand(
                currentOwner.OwnerId,
                budgetPlanId,
                savingContributionId);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }
}
