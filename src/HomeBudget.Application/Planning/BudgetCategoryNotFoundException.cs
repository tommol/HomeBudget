namespace HomeBudget.Application.Planning;

/// <summary>
/// Represents an error that occurs when a budget category cannot be found.
/// </summary>
public sealed class BudgetCategoryNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BudgetCategoryNotFoundException"/> class.
    /// </summary>
    /// <param name="budgetCategoryId">The identifier of the budget category that was not found.</param>
    public BudgetCategoryNotFoundException(Guid budgetCategoryId)
        : base($"Budget category '{budgetCategoryId}' was not found.")
    {
        BudgetCategoryId = budgetCategoryId;
    }

    /// <summary>
    /// Gets the identifier of the budget category that was not found.
    /// </summary>
    public Guid BudgetCategoryId { get; }
}
