using HomeBudget.Domain.Kernel;

namespace HomeBudget.Infrastructure.Server.Persistence.Outbox;

/// <summary>
/// Stores an integration message that should be published after the database transaction commits.
/// </summary>
public sealed class OutboxMessage
{
    private OutboxMessage()
    {
        Type = string.Empty;
        Content = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxMessage"/> class.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="type">The message type.</param>
    /// <param name="content">The serialized message content.</param>
    /// <param name="occurredOnUtc">The time when the source event occurred.</param>
    public OutboxMessage(
        Guid id,
        string type,
        string content,
        DateTimeOffset occurredOnUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Outbox message id cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Outbox message type is required.", nameof(type));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Outbox message content is required.", nameof(content));
        }

        Id = id;
        Type = type.Trim();
        Content = content;
        OccurredOnUtc = occurredOnUtc;
    }

    /// <summary>
    /// Gets the message identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the message type.
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Gets the serialized message content.
    /// </summary>
    public string Content { get; private set; }

    /// <summary>
    /// Gets the time when the source event occurred.
    /// </summary>
    public DateTimeOffset OccurredOnUtc { get; private set; }

    /// <summary>
    /// Gets the time when the message was processed.
    /// </summary>
    public DateTimeOffset? ProcessedOnUtc { get; private set; }

    /// <summary>
    /// Gets the last processing error.
    /// </summary>
    public string? Error { get; private set; }

    /// <summary>
    /// Creates an outbox message for a domain event.
    /// </summary>
    /// <param name="domainEvent">The source domain event.</param>
    /// <param name="content">The serialized message content.</param>
    /// <returns>The outbox message.</returns>
    public static OutboxMessage FromDomainEvent(IDomainEvent domainEvent, string content)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        return new OutboxMessage(
            Guid.NewGuid(),
            domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
            content,
            domainEvent.OccurredOnUtc);
    }

    /// <summary>
    /// Marks the message as processed.
    /// </summary>
    /// <param name="processedOnUtc">The processing time.</param>
    public void MarkProcessed(DateTimeOffset processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        Error = null;
    }

    /// <summary>
    /// Marks the message as failed.
    /// </summary>
    /// <param name="error">The processing error.</param>
    public void MarkFailed(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentException("Outbox error is required.", nameof(error));
        }

        Error = error.Trim();
    }
}
