using Asp.Versioning;
using HomeBudget.Api.Auth;
using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Execution.CloseBudget;

namespace HomeBudget.Api.Endpoints.Execution;

internal static class CloseBudgetEndpoint
{
    public static IEndpointRouteBuilder MapCloseBudgetEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{budgetId:guid}/close", HandleAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("CloseBudget")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid budgetId,
        ICurrentOwner currentOwner,
        ICommandHandler<CloseBudgetCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await ExecutionEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new CloseBudgetCommand(currentOwner.OwnerId, budgetId);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }
}
