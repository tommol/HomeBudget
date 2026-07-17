using Asp.Versioning;
using HomeBudget.Api.Auth;
using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Planning.ChangeSavingContributionAmount;
using HomeBudget.Contracts.Planning;

namespace HomeBudget.Api.Endpoints.Planning;

internal static class ChangeSavingContributionAmountEndpoint
{
    public static IEndpointRouteBuilder MapChangeSavingContributionAmountEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPatch(
                "/{budgetPlanId:guid}/saving-contributions/{savingContributionId:guid}/amount",
                HandleAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("ChangeSavingContributionAmount")
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
        ChangeSavingContributionAmountRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<ChangeSavingContributionAmountCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await PlanningEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new ChangeSavingContributionAmountCommand(
                currentOwner.OwnerId,
                budgetPlanId,
                savingContributionId,
                request.Amount);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }
}
