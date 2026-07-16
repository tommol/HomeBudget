namespace HomeBudget.Domain.Kernel;

/// <summary>
/// Represents a strongly typed identifier in the domain model.
/// </summary>
/// <typeparam name="TValue">The type of the underlying value.</typeparam>
public interface IStronglyTypedId<out TValue>
    where TValue : notnull
{
    /// <summary>
    /// Gets the underlying identifier value.
    /// </summary>
    TValue Value { get; }
}
