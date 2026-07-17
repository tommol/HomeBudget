using HomeBudget.Domain.Execution;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Execution;

/// <summary>
/// Provides persistence operations for executed budgets.
/// </summary>
public interface IBudgetRepository
{
    /// <summary>
    /// Gets a budget by its identifier.
    /// </summary>
    /// <param name="id">The budget identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching budget, or <c>null</c> when it was not found.</returns>
    Task<Budget?> GetByIdAsync(BudgetId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a budget by its identifier and owner.
    /// </summary>
    /// <param name="id">The budget identifier.</param>
    /// <param name="ownerId">The budget owner identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching budget, or <c>null</c> when it was not found for the owner.</returns>
    Task<Budget?> GetByIdAndOwnerIdAsync(
        BudgetId id,
        OwnerId ownerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a budget to the repository.
    /// </summary>
    /// <param name="budget">The budget to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAsync(Budget budget, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing budget in the repository.
    /// </summary>
    /// <param name="budget">The budget to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAsync(Budget budget, CancellationToken cancellationToken = default);
}
