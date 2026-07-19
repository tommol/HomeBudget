using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Execution;
using HomeBudget.Application.Planning;
using HomeBudget.Domain.Execution;
using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;
using HomeBudget.Infrastructure.Server;
using HomeBudget.Infrastructure.Server.Persistence;
using HomeBudget.Infrastructure.Server.Persistence.Outbox;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HomeBudget.Infrastructure.Server.Tests.Persistence;

public sealed class HomeBudgetDbContextTests
{
    [Fact]
    public async Task SaveChangesAsync_DispatchesDomainEventsAndClearsThem()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var dispatcher = new RecordingDomainEventDispatcher();
        var options = CreateOptions(connection);

        await using var dbContext = new HomeBudgetDbContext(options, dispatcher);
        await dbContext.Database.EnsureCreatedAsync();

        var ownerId = new OwnerId(Guid.NewGuid());
        var budgetPlan = CreateBudgetPlan(ownerId);
        var incomeCategory = CreateCategory(ownerId, BudgetCategoryType.Income);

        dbContext.BudgetPlans.Add(budgetPlan);
        dbContext.BudgetCategories.Add(incomeCategory);
        await dbContext.SaveChangesAsync();

        budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            incomeCategory,
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10));

        await ((IUnitOfWork)dbContext).SaveChangesAsync();

        Assert.Single(dispatcher.DispatchedEvents);
        Assert.IsType<PlannedIncomeAddedEvent>(dispatcher.DispatchedEvents[0]);
        Assert.Empty(budgetPlan.DomainEvents);
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsAggregateChangesAndOutboxMessagesInOneTransaction()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var serviceProvider = CreateServiceProvider(
            connection,
            services => services.AddScoped<IDomainEventHandler<PlannedIncomeAddedEvent>, PlannedIncomeOutboxHandler>());

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var ownerId = new OwnerId(Guid.NewGuid());
        var budgetPlan = CreateBudgetPlan(ownerId);
        var incomeCategory = CreateCategory(ownerId, BudgetCategoryType.Income);

        dbContext.BudgetPlans.Add(budgetPlan);
        dbContext.BudgetCategories.Add(incomeCategory);
        await dbContext.SaveChangesAsync();

        budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            incomeCategory,
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10));

        await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync();

        var reloadedBudgetPlan = await dbContext.BudgetPlans
            .SingleAsync(plan => plan.Id == budgetPlan.Id);
        var outboxMessage = await dbContext.OutboxMessages.SingleAsync();

        Assert.Single(reloadedBudgetPlan.PlannedIncomes);
        Assert.Equal(typeof(PlannedIncomeAddedEvent).FullName, outboxMessage.Type);
        Assert.Contains(budgetPlan.Id.Value.ToString(), outboxMessage.Content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SaveChangesAsync_RollsBackAggregateChanges_WhenDomainEventHandlerFails()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var setupDbContext = new HomeBudgetDbContext(
            CreateOptions(connection),
            new RecordingDomainEventDispatcher());
        await setupDbContext.Database.EnsureCreatedAsync();

        var ownerId = new OwnerId(Guid.NewGuid());
        var budgetPlan = CreateBudgetPlan(ownerId);
        var incomeCategory = CreateCategory(ownerId, BudgetCategoryType.Income);

        setupDbContext.BudgetPlans.Add(budgetPlan);
        setupDbContext.BudgetCategories.Add(incomeCategory);
        await setupDbContext.SaveChangesAsync();

        await using (var failingServiceProvider = CreateServiceProvider(
                         connection,
                         services => services.AddScoped<IDomainEventHandler<PlannedIncomeAddedEvent>, FailingPlannedIncomeHandler>()))
        {
            using var failingScope = failingServiceProvider.CreateScope();
            var failingDbContext = failingScope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
            var persistedBudgetPlan = await failingDbContext.BudgetPlans
                .SingleAsync(plan => plan.Id == budgetPlan.Id);
            var persistedCategory = await failingDbContext.BudgetCategories
                .SingleAsync(category => category.Id == incomeCategory.Id);

            persistedBudgetPlan.AddPlannedIncome(
                new PlannedIncomeId(Guid.NewGuid()),
                persistedCategory,
                "Salary",
                new Money(5000m, Currency.PLN),
                new DateOnly(2026, 7, 10));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => failingScope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync());
        }

        await using var verificationDbContext = new HomeBudgetDbContext(
            CreateOptions(connection),
            new RecordingDomainEventDispatcher());

        var reloadedBudgetPlan = await verificationDbContext.BudgetPlans
            .SingleAsync(plan => plan.Id == budgetPlan.Id);

        Assert.Empty(reloadedBudgetPlan.PlannedIncomes);
        Assert.Empty(await verificationDbContext.OutboxMessages.ToArrayAsync());
    }

    [Fact]
    public async Task Repositories_ReturnOnlyEntitiesMatchingOwner()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var serviceProvider = CreateServiceProvider(connection, _ => { });

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var ownerId = new OwnerId(Guid.NewGuid());
        var otherOwnerId = new OwnerId(Guid.NewGuid());
        var budgetPlan = CreateBudgetPlan(ownerId);
        var otherBudgetPlan = CreateBudgetPlan(otherOwnerId);
        var category = CreateCategory(ownerId, BudgetCategoryType.Expense);
        var otherCategory = CreateCategory(otherOwnerId, BudgetCategoryType.Expense);

        dbContext.BudgetPlans.AddRange(budgetPlan, otherBudgetPlan);
        dbContext.BudgetCategories.AddRange(category, otherCategory);
        await dbContext.SaveChangesAsync();

        var budgetPlanRepository = scope.ServiceProvider.GetRequiredService<IBudgetPlanRepository>();
        var categoryRepository = scope.ServiceProvider.GetRequiredService<IBudgetCategoryRepository>();

        Assert.NotNull(await budgetPlanRepository.GetByIdAndOwnerIdAsync(budgetPlan.Id, ownerId));
        Assert.Null(await budgetPlanRepository.GetByIdAndOwnerIdAsync(budgetPlan.Id, otherOwnerId));
        Assert.NotNull(await categoryRepository.GetByIdAndOwnerIdAsync(category.Id, ownerId));
        Assert.Null(await categoryRepository.GetByIdAndOwnerIdAsync(category.Id, otherOwnerId));
    }

    [Fact]
    public async Task SaveChangesAsync_CreatesBudget_WhenBudgetPlanIsActivated()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var serviceProvider = CreateServiceProvider(connection, _ => { });

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var ownerId = new OwnerId(Guid.NewGuid());
        var budgetPlan = CreateBudgetPlan(ownerId);

        dbContext.BudgetPlans.Add(budgetPlan);
        await dbContext.SaveChangesAsync();

        budgetPlan.Activate();

        await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync();

        var budget = await dbContext.Budgets.SingleAsync();

        Assert.Equal(new BudgetId(budgetPlan.Id.Value), budget.Id);
        Assert.Equal(budgetPlan.Id, budget.SourceBudgetPlanId);
        Assert.Equal(budgetPlan.OwnerId, budget.OwnerId);
        Assert.Equal(budgetPlan.Period, budget.Period);
        Assert.Equal(budgetPlan.DefaultCurrency, budget.DefaultCurrency);
        Assert.Equal(BudgetStatus.Active, budget.Status);
    }

    private static DbContextOptions<HomeBudgetDbContext> CreateOptions(SqliteConnection connection)
        => new DbContextOptionsBuilder<HomeBudgetDbContext>()
            .UseSqlite(connection)
            .Options;

    private static ServiceProvider CreateServiceProvider(
        SqliteConnection connection,
        Action<IServiceCollection> configureServices)
    {
        var services = new ServiceCollection();

        services.AddServerInfrastructure(options => options.UseSqlite(connection));
        configureServices(services);

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static BudgetPlan CreateBudgetPlan(OwnerId ownerId)
        => new(
            new BudgetPlanId(Guid.NewGuid()),
            ownerId,
            new BudgetPeriod(2026, 7),
            Currency.PLN);

    private static BudgetCategory CreateCategory(OwnerId ownerId, BudgetCategoryType type)
        => new(
            new BudgetCategoryId(Guid.NewGuid()),
            ownerId,
            "Category",
            type);

    private sealed class RecordingDomainEventDispatcher : IDomainEventDispatcher
    {
        public List<IDomainEvent> DispatchedEvents { get; } = [];

        public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            DispatchedEvents.Add(domainEvent);

            return Task.CompletedTask;
        }
    }

    private sealed class PlannedIncomeOutboxHandler : IDomainEventHandler<PlannedIncomeAddedEvent>
    {
        private readonly HomeBudgetDbContext _dbContext;

        public PlannedIncomeOutboxHandler(HomeBudgetDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task HandleAsync(PlannedIncomeAddedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            var content = $$"""
                {
                  "budgetPlanId": "{{domainEvent.BudgetPlanId.Value}}",
                  "plannedIncomeId": "{{domainEvent.PlannedIncomeId.Value}}"
                }
                """;

            _dbContext.OutboxMessages.Add(OutboxMessage.FromDomainEvent(domainEvent, content));

            return Task.CompletedTask;
        }
    }

    private sealed class FailingPlannedIncomeHandler : IDomainEventHandler<PlannedIncomeAddedEvent>
    {
        public Task HandleAsync(PlannedIncomeAddedEvent domainEvent, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Domain event handler failed.");
    }
}
