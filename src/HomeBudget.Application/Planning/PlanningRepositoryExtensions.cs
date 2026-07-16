using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Planning;

internal static class PlanningRepositoryExtensions
{
    /// <summary>
    /// Gets a required budget plan by its Guid identifier.
    /// </summary>
    /// <param name="repository">The budget plan repository.</param>
    /// <param name="budgetPlanId">The budget plan Guid identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching budget plan.</returns>
    public static async Task<BudgetPlan> GetRequiredByIdAsync(
        this IBudgetPlanRepository repository,
        Guid budgetPlanId,
        CancellationToken cancellationToken = default)
    {
        var id = new BudgetPlanId(budgetPlanId);

        return await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new BudgetPlanNotFoundException(budgetPlanId);
    }

    /// <summary>
    /// Gets a required budget category by its Guid identifier.
    /// </summary>
    /// <param name="repository">The budget category repository.</param>
    /// <param name="budgetCategoryId">The budget category Guid identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching budget category.</returns>
    public static async Task<BudgetCategory> GetRequiredByIdAsync(
        this IBudgetCategoryRepository repository,
        Guid budgetCategoryId,
        CancellationToken cancellationToken = default)
    {
        var id = new BudgetCategoryId(budgetCategoryId);

        return await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new BudgetCategoryNotFoundException(budgetCategoryId);
    }
}
