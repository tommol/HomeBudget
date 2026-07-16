namespace HomeBudget.Application.Abstractions;

/// <summary>
/// Handles an application query.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle.</typeparam>
/// <typeparam name="TResult">The type of the query result.</typeparam>
public interface IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Handles the specified query.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The query result.</returns>
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
