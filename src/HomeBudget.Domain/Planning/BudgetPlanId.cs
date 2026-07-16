using HomeBudget.Domain.Kernel;

namespace HomeBudget.Domain.Planning;

/// <summary>
/// Represents a strongly typed identifier for a BudgetPlan entity.
/// </summary>
/// <param name="Value">The unique value for the identifier.</param>
public sealed record BudgetPlanId(Guid Value) : IStronglyTypedId<Guid>
{
    public Guid Value { get; } = Value == Guid.Empty
        ? throw new ArgumentException("Budget plan id cannot be empty.", nameof(Value))
        : Value;
}
