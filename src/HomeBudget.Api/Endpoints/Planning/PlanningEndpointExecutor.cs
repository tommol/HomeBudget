using HomeBudget.Application.Planning;

namespace HomeBudget.Api.Endpoints.Planning;

internal static class PlanningEndpointExecutor
{
    public static async Task<IResult> ExecuteAsync(Func<Task<IResult>> command)
    {
        try
        {
            return await command();
        }
        catch (BudgetPlanNotFoundException exception)
        {
            return TypedResults.Problem(
                title: "Budget plan was not found.",
                detail: exception.Message,
                statusCode: StatusCodes.Status404NotFound);
        }
        catch (BudgetCategoryNotFoundException exception)
        {
            return TypedResults.Problem(
                title: "Budget category was not found.",
                detail: exception.Message,
                statusCode: StatusCodes.Status404NotFound);
        }
        catch (ArgumentException exception)
        {
            return TypedResults.Problem(
                title: "Invalid request.",
                detail: exception.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
        catch (InvalidOperationException exception)
        {
            return TypedResults.Problem(
                title: "Invalid request.",
                detail: exception.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
