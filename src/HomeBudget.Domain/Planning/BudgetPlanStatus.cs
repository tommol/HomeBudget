namespace HomeBudget.Domain.Planning;

/// <summary>
/// Represents the status of a budget plan.
/// </summary>
public enum BudgetPlanStatus
{
    /// <summary>
    /// The budget plan is in draft status and can be modified.
    /// </summary>
    Draft,

    /// <summary>
    /// The budget plan has been approved and is active.
    /// </summary>
    Active,

    /// <summary>
    /// The budget plan has been closed and is no longer active.
    /// </summary>
    Closed
}
