namespace HomeBudget.Domain.Planning;

/// <summary>
/// Describes how planned expense allocations fit against planned income.
/// </summary>
public enum BudgetFitRisk
{
    /// <summary>
    /// Indicates that all planned expense allocations fit within planned income.
    /// </summary>
    Balanced = 0,

    /// <summary>
    /// Indicates that only optional allocations need to be reduced or removed.
    /// </summary>
    OptionalOverrun = 1,

    /// <summary>
    /// Indicates that flexible allocations need to be reduced after optional ones.
    /// </summary>
    FlexibleOverrun = 2,

    /// <summary>
    /// Indicates that fixed allocations alone exceed planned income.
    /// </summary>
    FixedOverrun = 3
}
