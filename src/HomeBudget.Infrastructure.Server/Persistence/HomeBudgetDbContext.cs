using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;
using HomeBudget.Infrastructure.Server.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace HomeBudget.Infrastructure.Server.Persistence;

/// <summary>
/// EF Core database context for server-side persistence.
/// </summary>
public sealed class HomeBudgetDbContext : DbContext, IUnitOfWork
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeBudgetDbContext"/> class.
    /// </summary>
    /// <param name="options">The EF Core options.</param>
    /// <param name="domainEventDispatcher">The domain event dispatcher.</param>
    public HomeBudgetDbContext(
        DbContextOptions<HomeBudgetDbContext> options,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(domainEventDispatcher);

        _domainEventDispatcher = domainEventDispatcher;
    }

    /// <summary>
    /// Gets the budget plans set.
    /// </summary>
    public DbSet<BudgetPlan> BudgetPlans => Set<BudgetPlan>();

    /// <summary>
    /// Gets the budget categories set.
    /// </summary>
    public DbSet<BudgetCategory> BudgetCategories => Set<BudgetCategory>();

    /// <summary>
    /// Gets the outbox messages set.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        var executionStrategy = Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await Database.BeginTransactionAsync(cancellationToken);

            var savedEntries = 0;

            while (true)
            {
                savedEntries += await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken)
                    .ConfigureAwait(false);

                var domainEvents = CollectDomainEvents();

                if (domainEvents.Count == 0)
                {
                    break;
                }

                foreach (var domainEvent in domainEvents)
                {
                    await _domainEventDispatcher.DispatchAsync(domainEvent, cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return savedEntries;
        }).ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HomeBudgetDbContext).Assembly);
    }

    private IReadOnlyCollection<IDomainEvent> CollectDomainEvents()
    {
        var aggregateRoots = ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToArray();

        var domainEvents = aggregateRoots
            .SelectMany(entity => entity.DomainEvents)
            .ToArray();

        foreach (var aggregateRoot in aggregateRoots)
        {
            aggregateRoot.ClearDomainEvents();
        }

        return domainEvents;
    }
}
