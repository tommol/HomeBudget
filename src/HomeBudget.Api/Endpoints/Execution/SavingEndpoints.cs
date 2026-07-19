using Asp.Versioning;
using HomeBudget.Api.Auth;
using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Execution.AddSaving;
using HomeBudget.Application.Execution.ChangeSavingAmount;
using HomeBudget.Application.Execution.ChangeSavingCategory;
using HomeBudget.Application.Execution.ChangeSavingOccurredDate;
using HomeBudget.Application.Execution.ChangeSavingTitle;
using HomeBudget.Application.Execution.RemoveSaving;
using HomeBudget.Contracts.Execution;
using Microsoft.AspNetCore.Mvc;

namespace HomeBudget.Api.Endpoints.Execution;

internal static class SavingEndpoints
{
    public static IEndpointRouteBuilder MapSavingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{budgetId:guid}/savings", AddAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("AddSaving")
            .Produces<AddSavingResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        app.MapPatch("/{budgetId:guid}/savings/{savingId:guid}/amount", ChangeAmountAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("ChangeSavingAmount")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        app.MapPatch("/{budgetId:guid}/savings/{savingId:guid}/category", ChangeCategoryAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("ChangeSavingCategory")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        app.MapPatch("/{budgetId:guid}/savings/{savingId:guid}/title", ChangeTitleAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("ChangeSavingTitle")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        app.MapPatch("/{budgetId:guid}/savings/{savingId:guid}/occurred-date", ChangeOccurredDateAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("ChangeSavingOccurredDate")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        app.MapDelete("/{budgetId:guid}/savings/{savingId:guid}", RemoveAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("RemoveSaving")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    private static async Task<IResult> AddAsync(
        Guid budgetId,
        AddSavingRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<AddSavingCommand, Guid> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await ExecutionEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new AddSavingCommand(
                currentOwner.OwnerId,
                budgetId,
                request.CategoryId,
                request.Title,
                request.Amount,
                request.CurrencyCode,
                request.OccurredDate,
                request.ConvertedAmount,
                request.ConversionDate);

            var savingId = await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.Created(
                $"/api/v1/execution/budgets/{budgetId}/savings/{savingId}",
                new AddSavingResponse(savingId));
        });
    }

    private static async Task<IResult> ChangeAmountAsync(
        Guid budgetId,
        Guid savingId,
        ChangeSavingAmountRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<ChangeSavingAmountCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await ExecutionEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new ChangeSavingAmountCommand(
                currentOwner.OwnerId,
                budgetId,
                savingId,
                request.Amount,
                request.CurrencyCode,
                request.ConvertedAmount,
                request.ConversionDate);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }

    private static async Task<IResult> ChangeCategoryAsync(
        Guid budgetId,
        Guid savingId,
        ChangeSavingCategoryRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<ChangeSavingCategoryCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await ExecutionEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new ChangeSavingCategoryCommand(
                currentOwner.OwnerId,
                budgetId,
                savingId,
                request.CategoryId);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }

    private static async Task<IResult> ChangeTitleAsync(
        Guid budgetId,
        Guid savingId,
        ChangeSavingTitleRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<ChangeSavingTitleCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await ExecutionEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new ChangeSavingTitleCommand(
                currentOwner.OwnerId,
                budgetId,
                savingId,
                request.Title);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }

    private static async Task<IResult> ChangeOccurredDateAsync(
        Guid budgetId,
        Guid savingId,
        ChangeSavingOccurredDateRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<ChangeSavingOccurredDateCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await ExecutionEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new ChangeSavingOccurredDateCommand(
                currentOwner.OwnerId,
                budgetId,
                savingId,
                request.OccurredDate);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }

    private static async Task<IResult> RemoveAsync(
        Guid budgetId,
        Guid savingId,
        [FromBody] RemoveSavingRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<RemoveSavingCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await ExecutionEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new RemoveSavingCommand(
                currentOwner.OwnerId,
                budgetId,
                savingId,
                request.RemovalReason);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }
}
