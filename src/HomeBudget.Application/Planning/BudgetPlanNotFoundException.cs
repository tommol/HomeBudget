namespace HomeBudget.Application.Planning;

/// <summary>
/// Represents an error that occurs when a budget plan cannot be found.
/// </summary>
public sealed class BudgetPlanNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BudgetPlanNotFoundException"/> class.
    /// </summary>
    /// <param name="budgetPlanId">The identifier of the budget plan that was not found.</param>
    public BudgetPlanNotFoundException(Guid budgetPlanId)
        : base($"Budget plan '{budgetPlanId}' was not found.")
    {
        BudgetPlanId = budgetPlanId;
    }

    /// <summary>
    /// Gets the identifier of the budget plan that was not found.
    /// </summary>
    public Guid BudgetPlanId { get; }
}
