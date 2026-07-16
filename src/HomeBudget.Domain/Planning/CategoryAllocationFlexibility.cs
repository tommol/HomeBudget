namespace HomeBudget.Domain.Planning;

/// <summary>
/// Defines how easily an expense category allocation can be reduced.
/// </summary>
public enum CategoryAllocationFlexibility
{
    /// <summary>
    /// Indicates a required expense that should be covered first.
    /// </summary>
    Fixed = 0,

    /// <summary>
    /// Indicates an expense that can be reduced if the budget does not fit.
    /// </summary>
    Flexible = 1,

    /// <summary>
    /// Indicates an expense that can be removed first when balancing the budget.
    /// </summary>
    Optional = 2
}
