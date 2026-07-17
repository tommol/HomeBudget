using HomeBudget.Domain.Kernel;

namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents a strongly typed identifier for a Saving entity.
/// </summary>
/// <param name="Value">The underlying value for the identifier.</param>
public sealed record SavingId(Guid Value) : IStronglyTypedId<Guid>
{
    /// <summary>
    /// Gets the underlying value for the identifier.
    /// </summary>
    public Guid Value { get; } = Value == Guid.Empty
        ? throw new ArgumentException("Saving id cannot be empty.", nameof(Value))
        : Value;
}
