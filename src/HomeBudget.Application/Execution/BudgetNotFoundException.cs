namespace HomeBudget.Application.Execution;

/// <summary>
/// Represents an error that occurs when a budget cannot be found.
/// </summary>
public sealed class BudgetNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BudgetNotFoundException"/> class.
    /// </summary>
    /// <param name="budgetId">The identifier of the budget that was not found.</param>
    public BudgetNotFoundException(Guid budgetId)
        : base($"Budget '{budgetId}' was not found.")
    {
        BudgetId = budgetId;
    }

    /// <summary>
    /// Gets the identifier of the budget that was not found.
    /// </summary>
    public Guid BudgetId { get; }
}
