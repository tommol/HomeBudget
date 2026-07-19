using Asp.Versioning;
using HomeBudget.Api.Auth;
using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Execution.AddIncome;
using HomeBudget.Application.Execution.ChangeIncomeAmount;
using HomeBudget.Application.Execution.ChangeIncomeCategory;
using HomeBudget.Application.Execution.ChangeIncomeOccurredDate;
using HomeBudget.Application.Execution.ChangeIncomeTitle;
using HomeBudget.Application.Execution.RemoveIncome;
using HomeBudget.Contracts.Execution;
using Microsoft.AspNetCore.Mvc;

namespace HomeBudget.Api.Endpoints.Execution;

internal static class IncomeEndpoints
{
    public static IEndpointRouteBuilder MapIncomeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{budgetId:guid}/incomes", AddAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("AddIncome")
            .Produces<AddIncomeResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        app.MapPatch("/{budgetId:guid}/incomes/{incomeId:guid}/amount", ChangeAmountAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("ChangeIncomeAmount")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        app.MapPatch("/{budgetId:guid}/incomes/{incomeId:guid}/category", ChangeCategoryAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("ChangeIncomeCategory")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        app.MapPatch("/{budgetId:guid}/incomes/{incomeId:guid}/title", ChangeTitleAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("ChangeIncomeTitle")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        app.MapPatch("/{budgetId:guid}/incomes/{incomeId:guid}/occurred-date", ChangeOccurredDateAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("ChangeIncomeOccurredDate")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        app.MapDelete("/{budgetId:guid}/incomes/{incomeId:guid}", RemoveAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("RemoveIncome")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    private static async Task<IResult> AddAsync(
        Guid budgetId,
        AddIncomeRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<AddIncomeCommand, Guid> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await ExecutionEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new AddIncomeCommand(
                currentOwner.OwnerId,
                budgetId,
                request.CategoryId,
                request.Title,
                request.Amount,
                request.CurrencyCode,
                request.OccurredDate,
                request.ConvertedAmount,
                request.ConversionDate);

            var incomeId = await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.Created(
                $"/api/v1/execution/budgets/{budgetId}/incomes/{incomeId}",
                new AddIncomeResponse(incomeId));
        });
    }

    private static async Task<IResult> ChangeAmountAsync(
        Guid budgetId,
        Guid incomeId,
        ChangeIncomeAmountRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<ChangeIncomeAmountCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await ExecutionEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new ChangeIncomeAmountCommand(
                currentOwner.OwnerId,
                budgetId,
                incomeId,
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
        Guid incomeId,
        ChangeIncomeCategoryRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<ChangeIncomeCategoryCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await ExecutionEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new ChangeIncomeCategoryCommand(
                currentOwner.OwnerId,
                budgetId,
                incomeId,
                request.CategoryId);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }

    private static async Task<IResult> ChangeTitleAsync(
        Guid budgetId,
        Guid incomeId,
        ChangeIncomeTitleRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<ChangeIncomeTitleCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await ExecutionEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new ChangeIncomeTitleCommand(
                currentOwner.OwnerId,
                budgetId,
                incomeId,
                request.Title);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }

    private static async Task<IResult> ChangeOccurredDateAsync(
        Guid budgetId,
        Guid incomeId,
        ChangeIncomeOccurredDateRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<ChangeIncomeOccurredDateCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await ExecutionEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new ChangeIncomeOccurredDateCommand(
                currentOwner.OwnerId,
                budgetId,
                incomeId,
                request.OccurredDate);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }

    private static async Task<IResult> RemoveAsync(
        Guid budgetId,
        Guid incomeId,
        [FromBody] RemoveIncomeRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<RemoveIncomeCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await ExecutionEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new RemoveIncomeCommand(
                currentOwner.OwnerId,
                budgetId,
                incomeId,
                request.RemovalReason);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }
}
