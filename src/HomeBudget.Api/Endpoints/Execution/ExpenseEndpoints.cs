using Asp.Versioning;
using HomeBudget.Api.Auth;
using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Execution.AddExpense;
using HomeBudget.Application.Execution.ChangeExpenseAmount;
using HomeBudget.Application.Execution.ChangeExpenseCategory;
using HomeBudget.Application.Execution.ChangeExpenseOccurredDate;
using HomeBudget.Application.Execution.ChangeExpenseTitle;
using HomeBudget.Application.Execution.RemoveExpense;
using HomeBudget.Contracts.Execution;
using Microsoft.AspNetCore.Mvc;

namespace HomeBudget.Api.Endpoints.Execution;

internal static class ExpenseEndpoints
{
    public static IEndpointRouteBuilder MapExpenseEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{budgetId:guid}/expenses", AddAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("AddExpense")
            .Produces<AddExpenseResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        app.MapPatch("/{budgetId:guid}/expenses/{expenseId:guid}/amount", ChangeAmountAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("ChangeExpenseAmount")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        app.MapPatch("/{budgetId:guid}/expenses/{expenseId:guid}/category", ChangeCategoryAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("ChangeExpenseCategory")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        app.MapPatch("/{budgetId:guid}/expenses/{expenseId:guid}/title", ChangeTitleAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("ChangeExpenseTitle")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        app.MapPatch("/{budgetId:guid}/expenses/{expenseId:guid}/occurred-date", ChangeOccurredDateAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("ChangeExpenseOccurredDate")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        app.MapDelete("/{budgetId:guid}/expenses/{expenseId:guid}", RemoveAsync)
            .MapToApiVersion(new ApiVersion(1))
            .WithGroupName("v1")
            .WithName("RemoveExpense")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    private static async Task<IResult> AddAsync(
        Guid budgetId,
        AddExpenseRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<AddExpenseCommand, Guid> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await ExecutionEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new AddExpenseCommand(
                currentOwner.OwnerId,
                budgetId,
                request.CategoryId,
                request.Title,
                request.Amount,
                request.CurrencyCode,
                request.OccurredDate,
                request.ConvertedAmount,
                request.ConversionDate);

            var expenseId = await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.Created(
                $"/api/v1/execution/budgets/{budgetId}/expenses/{expenseId}",
                new AddExpenseResponse(expenseId));
        });
    }

    private static async Task<IResult> ChangeAmountAsync(
        Guid budgetId,
        Guid expenseId,
        ChangeExpenseAmountRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<ChangeExpenseAmountCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await ExecutionEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new ChangeExpenseAmountCommand(
                currentOwner.OwnerId,
                budgetId,
                expenseId,
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
        Guid expenseId,
        ChangeExpenseCategoryRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<ChangeExpenseCategoryCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await ExecutionEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new ChangeExpenseCategoryCommand(
                currentOwner.OwnerId,
                budgetId,
                expenseId,
                request.CategoryId);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }

    private static async Task<IResult> ChangeTitleAsync(
        Guid budgetId,
        Guid expenseId,
        ChangeExpenseTitleRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<ChangeExpenseTitleCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await ExecutionEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new ChangeExpenseTitleCommand(
                currentOwner.OwnerId,
                budgetId,
                expenseId,
                request.Title);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }

    private static async Task<IResult> ChangeOccurredDateAsync(
        Guid budgetId,
        Guid expenseId,
        ChangeExpenseOccurredDateRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<ChangeExpenseOccurredDateCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await ExecutionEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new ChangeExpenseOccurredDateCommand(
                currentOwner.OwnerId,
                budgetId,
                expenseId,
                request.OccurredDate);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }

    private static async Task<IResult> RemoveAsync(
        Guid budgetId,
        Guid expenseId,
        [FromBody] RemoveExpenseRequest request,
        ICurrentOwner currentOwner,
        ICommandHandler<RemoveExpenseCommand> handler,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        return await ExecutionEndpointExecutor.ExecuteAsync(async () =>
        {
            var command = new RemoveExpenseCommand(
                currentOwner.OwnerId,
                budgetId,
                expenseId,
                request.RemovalReason);

            await handler.HandleAsync(command, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        });
    }
}
