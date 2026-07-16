namespace HomeBudget.Domain.Kernel;

/// <summary>
/// Represents a strongly typed identifier in the domain model.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Determines whether the specified value object is equal to the current value object.
    /// </summary>
    /// <param name="other">The value object to compare with the current value object.</param>
    /// <returns>true if the specified value object is equal to the current value object; otherwise, false.</returns>
    public bool Equals(ValueObject? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return GetType() == other.GetType()
            && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current value object.
    /// </summary>
    /// <param name="obj">The object to compare with the current value object.</param>
    /// <returns>true if the specified object is equal to the current value object; otherwise, false.</returns>
    public override bool Equals(object? obj) => obj is ValueObject other && Equals(other);

    /// <summary>
    /// Returns a hash code for the current value object.
    /// </summary>
    /// <returns>The hash code for the current value object.</returns>
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(GetType());

        foreach (var component in GetEqualityComponents())
        {
            hashCode.Add(component);
        }

        return hashCode.ToHashCode();
    }

    /// <summary>
    /// Determines whether two value objects are equal.
    /// </summary>
    /// <param name="left">The first value object to compare.</param>
    /// <param name="right">The second value object to compare.</param>
    /// <returns>true if the specified value objects are equal; otherwise, false.</returns>
    public static bool operator ==(ValueObject? left, ValueObject? right)
        => left?.Equals(right) ?? right is null;

    /// <summary>
    /// Determines whether two value objects are not equal.
    /// </summary>
    /// <param name="left">The first value object to compare.</param>
    /// <param name="right">The second value object to compare.</param>
    /// <returns>true if the specified value objects are not equal; otherwise, false.</returns>
    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !(left == right);

    /// <summary>
    /// Gets the component values used to determine value object equality.
    /// </summary>
    /// <returns>The values that define equality for the value object.</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();
}
