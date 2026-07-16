namespace HomeBudget.Domain.Kernel;

/// <summary>
/// Represents an entity in the domain model.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    protected Entity(TId id)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default!))
        {
            throw new ArgumentException("Entity id cannot be the default value.", nameof(id));
        }

        Id = id;
    }

    /// <summary>
    /// Gets the identifier of the entity.
    /// </summary>
    public TId Id { get; }

    /// <summary>
    /// Determines whether the specified entity is equal to the current entity.
    /// </summary>
    /// <param name="other">The entity to compare with the current entity.</param>
    /// <returns><c>true</c> if the specified entity is equal to the current entity; otherwise, <c>false</c>.</returns>
    public bool Equals(Entity<TId>? other)
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
            && EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current entity.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns><c>true</c> if the specified object is equal to the current entity; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj) => obj is Entity<TId> other && Equals(other);

    /// <summary>
    /// Returns a hash code for the current entity.
    /// </summary>
    /// <returns>The hash code for the current entity.</returns>
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    /// <summary>
    /// Determines whether two entities are equal.
    /// </summary>
    /// <param name="left">The first entity to compare.</param>
    /// <param name="right">The second entity to compare.</param>
    /// <returns><c>true</c> if the entities are equal; otherwise, <c>false</c>.</returns>  
    /// <remarks>
    /// This operator checks for equality between two entities based on their identifiers and types.
    /// It returns <c>true</c> if both entities are of the same type and have the same identifier; otherwise, it returns <c>false</c>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when either <paramref name="left"/> or <paramref name="right"/> is <c>null</c>.</exception>
    /// <example>
    /// <code>
    /// var entity1 = new SomeEntity(1);
    /// var entity2 = new SomeEntity(1);
    /// var entity3 = new SomeEntity(2);
    ///
    /// bool areEqual1 = entity1 == entity2; // true
    /// bool areEqual2 = entity1 == entity3; // false
    /// </code>
    /// </example>  
    /// <seealso cref="Equals(Entity{TId})"/>
    /// <seealso cref="Equals(object)"/>
    /// <seealso cref="GetHashCode()"/>
    /// <seealso cref="!=(Entity{TId}, Entity{TId})"
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => left?.Equals(right) ?? right is null;

    /// <summary>
    /// Determines whether two entities are not equal.
    /// </summary>
    /// <param name="left">The first entity to compare.</param>
    /// <param name="right">The second entity to compare.</param>
    /// <returns><c>true</c> if the entities are not equal; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This operator checks for inequality between two entities based on their identifiers and types.
    /// It returns <c>true</c> if the entities are of different types or have different identifiers; otherwise, it returns <c>false</c>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when either <paramref name="left"/> or <paramref name="right"/> is <c>null</c>.</exception>
    /// <example>
    /// <code>
    /// var entity1 = new SomeEntity(1);
    /// var entity2 = new SomeEntity(1);
    /// var entity3 = new SomeEntity(2);
    /// 
    /// bool areNotEqual1 = entity1 != entity2; // false
    /// bool areNotEqual2 = entity1 != entity3; // true
    /// </code>
    /// </example>
    /// <seealso cref="Equals(Entity{TId})"/>
    /// <seealso cref="Equals(object)"/>
    /// <seealso cref="GetHashCode()"/>
    /// <seealso cref="==(Entity{TId}, Entity{TId})"/>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !(left == right);
}
