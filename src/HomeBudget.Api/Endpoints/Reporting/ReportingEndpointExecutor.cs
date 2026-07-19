using HomeBudget.Application.Reporting;

namespace HomeBudget.Api.Endpoints.Reporting;

internal static class ReportingEndpointExecutor
{
    public static async Task<IResult> ExecuteAsync(Func<Task<IResult>> query)
    {
        try
        {
            return await query();
        }
        catch (BudgetBalanceNotFoundException exception)
        {
            return TypedResults.Problem(
                title: "Budget balance was not found.",
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
