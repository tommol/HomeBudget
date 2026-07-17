using HomeBudget.Application.Planning;
using HomeBudget.Domain.Planning;
using Microsoft.EntityFrameworkCore;

namespace HomeBudget.Infrastructure.Server.Persistence.Repositories;

internal sealed class EfBudgetPlanRepository : IBudgetPlanRepository
{
    private readonly HomeBudgetDbContext _dbContext;

    public EfBudgetPlanRepository(HomeBudgetDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public Task<BudgetPlan?> GetByIdAsync(BudgetPlanId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _dbContext.BudgetPlans
            .SingleOrDefaultAsync(budgetPlan => budgetPlan.Id == id, cancellationToken);
    }

    public async Task AddAsync(BudgetPlan budgetPlan, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(budgetPlan);

        await _dbContext.BudgetPlans.AddAsync(budgetPlan, cancellationToken).ConfigureAwait(false);
    }

    public Task UpdateAsync(BudgetPlan budgetPlan, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(budgetPlan);

        _dbContext.BudgetPlans.Update(budgetPlan);

        return Task.CompletedTask;
    }
}
