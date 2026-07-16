using HomeBudget.Domain.Kernel;

namespace HomeBudget.Domain.Planning;

/// <summary>
/// Represents a strongly typed identifier for a PlannedIncome entity.
/// </summary>
/// <param name="Value">The underlying Guid value.</param>
public sealed record PlannedIncomeId(Guid Value) : IStronglyTypedId<Guid>
{
    /// <summary>
    /// Gets the underlying Guid value.
    /// </summary>
    public Guid Value { get; } = Value == Guid.Empty
        ? throw new ArgumentException("Planned income id cannot be empty.", nameof(Value))
        : Value;
}
