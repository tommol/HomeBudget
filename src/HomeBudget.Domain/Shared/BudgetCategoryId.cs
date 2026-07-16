using HomeBudget.Domain.Kernel;

namespace HomeBudget.Domain.Shared;

/// <summary>
/// Represents a strongly typed identifier for a BudgetCategory entity.
/// </summary>
/// <param name="Value">The underlying Guid value.</param>
public sealed record BudgetCategoryId(Guid Value) : IStronglyTypedId<Guid>
{
    public Guid Value { get; } = Value == Guid.Empty
        ? throw new ArgumentException("Budget category id cannot be empty.", nameof(Value))
        : Value;
}
