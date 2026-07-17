using HomeBudget.Application.Planning;
using HomeBudget.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace HomeBudget.Infrastructure.Server.Persistence.Repositories;

internal sealed class EfBudgetCategoryRepository : IBudgetCategoryRepository
{
    private readonly HomeBudgetDbContext _dbContext;

    public EfBudgetCategoryRepository(HomeBudgetDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public Task<BudgetCategory?> GetByIdAsync(BudgetCategoryId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _dbContext.BudgetCategories
            .SingleOrDefaultAsync(category => category.Id == id, cancellationToken);
    }
}
