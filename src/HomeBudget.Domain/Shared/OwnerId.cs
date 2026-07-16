using HomeBudget.Domain.Kernel;

namespace HomeBudget.Domain.Shared;

/// <summary>
/// Represents a strongly typed identifier for a budget owner.
/// </summary>
/// <param name="Value">The underlying Guid value.</param>
public sealed record OwnerId(Guid Value) : IStronglyTypedId<Guid>
{
    public Guid Value { get; } = Value == Guid.Empty
        ? throw new ArgumentException("Owner id cannot be empty.", nameof(Value))
        : Value;
}
