namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents the status of a budget.
/// </summary>
public enum BudgetStatus
{
    /// <summary>
    /// The budget is currently active and can be modified.
    /// </summary>
    Active,
    /// <summary>
    /// The budget is closed and cannot be modified.
    /// </summary>
    Closed
}