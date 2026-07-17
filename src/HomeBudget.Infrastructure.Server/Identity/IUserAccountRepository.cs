namespace HomeBudget.Infrastructure.Server.Identity;

/// <summary>
/// Provides lookup operations for technical user accounts.
/// </summary>
public interface IUserAccountRepository
{
    /// <summary>
    /// Gets a user account by external issuer and subject.
    /// </summary>
    /// <param name="issuer">The external token issuer.</param>
    /// <param name="subject">The external subject identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching user account, or <c>null</c> when it was not found.</returns>
    Task<UserAccount?> GetByIssuerAndSubjectAsync(
        string issuer,
        string subject,
        CancellationToken cancellationToken = default);
}
