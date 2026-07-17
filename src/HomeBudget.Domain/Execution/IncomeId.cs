using HomeBudget.Domain.Kernel;

namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents a strongly typed identifier for an Income entity.
/// </summary>
/// <param name="Value">The underlying value for the identifier.</param>
public sealed record IncomeId(Guid Value) : IStronglyTypedId<Guid>
{
    /// <summary>
    /// Gets the underlying value for the identifier.
    /// </summary>
    public Guid Value { get; } = Value == Guid.Empty
        ? throw new ArgumentException("Income id cannot be empty.", nameof(Value))
        : Value;
}