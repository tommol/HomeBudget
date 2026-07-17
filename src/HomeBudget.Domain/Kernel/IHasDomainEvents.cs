namespace HomeBudget.Domain.Kernel;

/// <summary>
/// Exposes domain events raised by an aggregate root.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Gets the domain events that have occurred.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears the domain events that have occurred.
    /// </summary>
    void ClearDomainEvents();
}
