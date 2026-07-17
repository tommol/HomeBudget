namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents the status of an executed budget entry.
/// </summary>
public enum BudgetEntryStatus
{
    /// <summary>
    /// The entry is active and included in budget totals.
    /// </summary>
    Active = 0,

    /// <summary>
    /// The entry was removed and is excluded from budget totals.
    /// </summary>
    Removed = 1
}
