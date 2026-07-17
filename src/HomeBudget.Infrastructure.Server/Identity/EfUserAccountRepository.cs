using HomeBudget.Infrastructure.Server.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HomeBudget.Infrastructure.Server.Identity;

internal sealed class EfUserAccountRepository : IUserAccountRepository
{
    private readonly HomeBudgetDbContext _dbContext;

    public EfUserAccountRepository(HomeBudgetDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public Task<UserAccount?> GetByIssuerAndSubjectAsync(
        string issuer,
        string subject,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new ArgumentException("Issuer is required.", nameof(issuer));
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Subject is required.", nameof(subject));
        }

        var normalizedIssuer = issuer.Trim();
        var normalizedSubject = subject.Trim();

        return _dbContext.UserAccounts
            .AsNoTracking()
            .SingleOrDefaultAsync(
                userAccount => userAccount.Issuer == normalizedIssuer
                    && userAccount.Subject == normalizedSubject,
                cancellationToken);
    }
}
