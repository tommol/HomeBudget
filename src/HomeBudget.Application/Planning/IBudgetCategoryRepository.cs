using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Planning;

/// <summary>
/// Provides persistence operations for budget categories.
/// </summary>
public interface IBudgetCategoryRepository
{
    /// <summary>
    /// Gets a budget category by its identifier.
    /// </summary>
    /// <param name="id">The budget category identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching budget category, or <c>null</c> when it was not found.</returns>
    Task<BudgetCategory?> GetByIdAsync(BudgetCategoryId id, CancellationToken cancellationToken = default);
}
