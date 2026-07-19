namespace HomeBudget.Application.Reporting;

/// <summary>
/// Represents an error that occurs when a budget balance cannot be found.
/// </summary>
public sealed class BudgetBalanceNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BudgetBalanceNotFoundException"/> class.
    /// </summary>
    /// <param name="year">The budget period year.</param>
    /// <param name="month">The budget period month.</param>
    public BudgetBalanceNotFoundException(int year, int month)
        : base($"Budget balance for period '{year:D4}-{month:D2}' was not found.")
    {
        Year = year;
        Month = month;
    }

    /// <summary>
    /// Gets the budget period year.
    /// </summary>
    public int Year { get; }

    /// <summary>
    /// Gets the budget period month.
    /// </summary>
    public int Month { get; }
}
