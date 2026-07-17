namespace HomeBudget.Application.Abstractions;

/// <summary>
/// Commits aggregate changes and domain event side effects atomically.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves pending changes, dispatches domain events, and commits the transaction.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
