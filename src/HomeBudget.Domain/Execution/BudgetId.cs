using HomeBudget.Domain.Kernel;

namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents a strongly typed identifier for a Budget entity.
/// </summary>
/// <param name="Value">The underlying Guid value.</param>
public sealed record BudgetId(Guid Value) : IStronglyTypedId<Guid>
{
   /// <summary>
    /// Gets the underlying Guid value.
    /// </summary>
    public Guid Value { get; } = Value == Guid.Empty
        ? throw new ArgumentException("Budget id cannot be empty.", nameof(Value))
        : Value;
}