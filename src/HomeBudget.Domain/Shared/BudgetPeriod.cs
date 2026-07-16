using HomeBudget.Domain.Kernel;

namespace HomeBudget.Domain.Shared;

/// <summary>
/// Represents a budget period with a year and month.
/// </summary>
public sealed class BudgetPeriod : ValueObject
{
    /// <summary>
    /// Gets the year of the budget period.
    /// </summary>
    public int Year { get; }

    /// <summary>
    /// Gets the month of the budget period.
    /// </summary>
    public int Month { get; }

    /// <summary>
    /// Gets the start date of the budget period.
    /// </summary>
    public DateOnly StartDate { get; }

    /// <summary>
    /// Gets the end date of the budget period.
    /// </summary>
    public DateOnly EndDate { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BudgetPeriod"/> class with the specified year and month.
    /// </summary>
    /// <param name="year">The year of the budget period.</param>
    /// <param name="month">The month of the budget period.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="month"/> is outside the 1-12 range.</exception>
    public BudgetPeriod(int year, int month)
    {
        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        }

        Year = year;
        Month = month;
        StartDate = new DateOnly(year, month, 1);
        EndDate = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
    }

    /// <summary>
    /// Gets the component values used to determine budget period equality.
    /// </summary>
    /// <returns>The year and month of the budget period.</returns>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Year;
        yield return Month;
    }
}
