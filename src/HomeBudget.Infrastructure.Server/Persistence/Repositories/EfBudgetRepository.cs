using HomeBudget.Application.Execution;
using HomeBudget.Domain.Execution;
using HomeBudget.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace HomeBudget.Infrastructure.Server.Persistence.Repositories;

internal sealed class EfBudgetRepository : IBudgetRepository
{
    private readonly HomeBudgetDbContext _dbContext;

    public EfBudgetRepository(HomeBudgetDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public Task<Budget?> GetByIdAsync(BudgetId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _dbContext.Budgets
            .SingleOrDefaultAsync(budget => budget.Id == id, cancellationToken);
    }

    public Task<Budget?> GetByIdAndOwnerIdAsync(
        BudgetId id,
        OwnerId ownerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(ownerId);

        return _dbContext.Budgets
            .SingleOrDefaultAsync(
                budget => budget.Id == id
                    && budget.OwnerId == ownerId,
                cancellationToken);
    }

    public async Task AddAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(budget);

        await _dbContext.Budgets.AddAsync(budget, cancellationToken).ConfigureAwait(false);
    }

    public Task UpdateAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(budget);

        _dbContext.Budgets.Update(budget);

        return Task.CompletedTask;
    }
}
