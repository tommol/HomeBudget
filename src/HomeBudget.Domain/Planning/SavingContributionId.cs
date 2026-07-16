using HomeBudget.Domain.Kernel;

namespace HomeBudget.Domain.Planning;

/// <summary>
/// Represents a strongly typed identifier for a SavingContribution entity.
/// </summary>
/// <param name="Value">The underlying Guid value.</param>
public sealed record SavingContributionId(Guid Value) : IStronglyTypedId<Guid>
{
    public Guid Value { get; } = Value == Guid.Empty
        ? throw new ArgumentException("Saving contribution id cannot be empty.", nameof(Value))
        : Value;
}
