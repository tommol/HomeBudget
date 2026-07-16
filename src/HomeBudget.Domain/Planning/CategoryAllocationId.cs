using HomeBudget.Domain.Kernel;

namespace HomeBudget.Domain.Planning;

/// <summary>
/// Represents a strongly typed identifier for a CategoryAllocation entity.
/// </summary>
/// <param name="Value">The underlying Guid value.</param>
public sealed record CategoryAllocationId(Guid Value) : IStronglyTypedId<Guid>
{
    public Guid Value { get; } = Value == Guid.Empty
        ? throw new ArgumentException("Category allocation id cannot be empty.", nameof(Value))
        : Value;
}
