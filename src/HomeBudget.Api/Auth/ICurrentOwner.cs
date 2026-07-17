namespace HomeBudget.Api.Auth;

/// <summary>
/// Provides the domain owner resolved for the current authenticated request.
/// </summary>
public interface ICurrentOwner
{
    /// <summary>
    /// Gets the current domain owner identifier.
    /// </summary>
    Guid OwnerId { get; }
}
