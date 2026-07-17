using HomeBudget.Domain.Execution;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Execution;

internal static class ExecutionRepositoryExtensions
{
    public static async Task<Budget> GetRequiredByIdAsync(
        this IBudgetRepository repository,
        Guid budgetId,
        CancellationToken cancellationToken = default)
    {
        var id = new BudgetId(budgetId);

        return await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new BudgetNotFoundException(budgetId);
    }

    public static async Task<Budget> GetRequiredByIdAsync(
        this IBudgetRepository repository,
        Guid budgetId,
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        var id = new BudgetId(budgetId);
        var owner = new OwnerId(ownerId);

        return await repository.GetByIdAndOwnerIdAsync(id, owner, cancellationToken)
            ?? throw new BudgetNotFoundException(budgetId);
    }
}
