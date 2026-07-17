using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Planning;

/// <summary>
/// Provides persistence operations for budget plans.
/// </summary>
public interface IBudgetPlanRepository
{
    /// <summary>
    /// Gets a budget plan by its identifier.
    /// </summary>
    /// <param name="id">The budget plan identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching budget plan, or <c>null</c> when it was not found.</returns>
    Task<BudgetPlan?> GetByIdAsync(BudgetPlanId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a budget plan by its identifier and owner.
    /// </summary>
    /// <param name="id">The budget plan identifier.</param>
    /// <param name="ownerId">The budget plan owner identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching budget plan, or <c>null</c> when it was not found for the owner.</returns>
    Task<BudgetPlan?> GetByIdAndOwnerIdAsync(
        BudgetPlanId id,
        OwnerId ownerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a budget plan to the repository.
    /// </summary>
    /// <param name="budgetPlan">The budget plan to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAsync(BudgetPlan budgetPlan, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing budget plan in the repository.
    /// </summary>
    /// <param name="budgetPlan">The budget plan to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAsync(BudgetPlan budgetPlan, CancellationToken cancellationToken = default);
}
