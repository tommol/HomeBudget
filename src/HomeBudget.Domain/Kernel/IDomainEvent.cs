namespace HomeBudget.Domain.Kernel;

/// <summary>
/// Represents a domain event in the domain model.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the date and time when the domain event occurred in UTC.
    /// </summary>
    DateTimeOffset OccurredOnUtc { get; }
}
