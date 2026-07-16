using HomeBudget.Domain.Planning;

namespace HomeBudget.Application.Planning;

internal static class PlanningCommandParsers
{
    /// <summary>
    /// Parses a category allocation flexibility value from a command string.
    /// </summary>
    /// <param name="flexibility">The flexibility value to parse.</param>
    /// <param name="parameterName">The command parameter name used in validation errors.</param>
    /// <returns>The parsed category allocation flexibility.</returns>
    public static CategoryAllocationFlexibility ParseCategoryAllocationFlexibility(
        string flexibility,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(flexibility))
        {
            throw new ArgumentException("Category allocation flexibility is required.", parameterName);
        }

        if (!Enum.TryParse<CategoryAllocationFlexibility>(
                flexibility.Trim(),
                ignoreCase: true,
                out var parsedFlexibility)
            || !Enum.IsDefined(parsedFlexibility))
        {
            throw new ArgumentException("Category allocation flexibility is invalid.", parameterName);
        }

        return parsedFlexibility;
    }
}
