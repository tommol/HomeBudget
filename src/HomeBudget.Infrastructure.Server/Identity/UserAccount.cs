using HomeBudget.Domain.Shared;

namespace HomeBudget.Infrastructure.Server.Identity;

/// <summary>
/// Represents a technical account mapped from an external identity provider user to a domain owner.
/// </summary>
public sealed class UserAccount
{
    private UserAccount()
    {
        OwnerId = null!;
        Issuer = string.Empty;
        Subject = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserAccount"/> class.
    /// </summary>
    /// <param name="id">The technical account identifier.</param>
    /// <param name="ownerId">The domain owner identifier mapped to this account.</param>
    /// <param name="issuer">The external token issuer.</param>
    /// <param name="subject">The external subject identifier.</param>
    /// <param name="email">The optional email address from the external identity provider.</param>
    /// <param name="displayName">The optional display name from the external identity provider.</param>
    /// <param name="createdAtUtc">The account creation timestamp.</param>
    public UserAccount(
        Guid id,
        OwnerId ownerId,
        string issuer,
        string subject,
        string? email = null,
        string? displayName = null,
        DateTimeOffset? createdAtUtc = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("User account id cannot be empty.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(ownerId);

        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new ArgumentException("Issuer is required.", nameof(issuer));
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Subject is required.", nameof(subject));
        }

        Id = id;
        OwnerId = ownerId;
        Issuer = issuer.Trim();
        Subject = subject.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        CreatedAtUtc = createdAtUtc ?? DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the technical account identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the domain owner identifier mapped to this account.
    /// </summary>
    public OwnerId OwnerId { get; private set; }

    /// <summary>
    /// Gets the external token issuer.
    /// </summary>
    public string Issuer { get; private set; }

    /// <summary>
    /// Gets the external subject identifier.
    /// </summary>
    public string Subject { get; private set; }

    /// <summary>
    /// Gets the optional email address from the external identity provider.
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// Gets the optional display name from the external identity provider.
    /// </summary>
    public string? DisplayName { get; private set; }

    /// <summary>
    /// Gets the account creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
