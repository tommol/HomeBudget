using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Execution;
using HomeBudget.Application.Planning;
using HomeBudget.Application.Reporting;
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

    [Fact]
    public async Task BudgetBalanceReadRepository_ReturnsFullBalance_WhenPlanAndBudgetExist()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var serviceProvider = CreateServiceProvider(connection, _ => { });

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var ownerId = new OwnerId(Guid.NewGuid());
        var period = new BudgetPeriod(2026, 7);
        var budgetPlan = CreateBudgetPlan(ownerId, period);
        var incomeCategory = CreateCategory(ownerId, BudgetCategoryType.Income);
        var expenseCategory = CreateCategory(ownerId, BudgetCategoryType.Expense);
        var savingCategory = CreateCategory(ownerId, BudgetCategoryType.Saving);
        budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            incomeCategory,
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10));
        budgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            expenseCategory,
            new Money(3000m, Currency.PLN),
            CategoryAllocationFlexibility.Fixed);
        budgetPlan.AddSavingContribution(
            new SavingContributionId(Guid.NewGuid()),
            savingCategory,
            new Money(500m, Currency.PLN));

        var budget = new Budget(
            new BudgetId(Guid.NewGuid()),
            ownerId,
            period,
            Currency.PLN,
            budgetPlan.Id);
        budget.AddIncome(
            new IncomeId(Guid.NewGuid()),
            incomeCategory,
            "Salary",
            new Money(5200m, Currency.PLN),
            new DateOnly(2026, 7, 10));
        budget.AddExpense(
            new ExpenseId(Guid.NewGuid()),
            expenseCategory,
            "Rent",
            new Money(2900m, Currency.PLN),
            new DateOnly(2026, 7, 11));
        budget.AddSaving(
            new SavingId(Guid.NewGuid()),
            savingCategory,
            "Emergency fund",
            new Money(600m, Currency.PLN),
            new DateOnly(2026, 7, 12));

        dbContext.BudgetCategories.AddRange(incomeCategory, expenseCategory, savingCategory);
        dbContext.BudgetPlans.Add(budgetPlan);
        dbContext.Budgets.Add(budget);
        await dbContext.SaveChangesAsync();

        var repository = scope.ServiceProvider.GetRequiredService<IBudgetBalanceReadRepository>();

        var balance = await repository.GetByOwnerIdAndPeriodAsync(ownerId, period);

        Assert.NotNull(balance);
        Assert.Equal(2026, balance.Year);
        Assert.Equal(7, balance.Month);
        Assert.Equal(budgetPlan.Id.Value, balance.BudgetPlanId);
        Assert.Equal(budget.Id.Value, balance.BudgetId);
        Assert.Equal("PLN", balance.CurrencyCode);
        Assert.Equal("Draft", balance.BudgetPlanStatus);
        Assert.Equal("Active", balance.BudgetStatus);
        Assert.Equal(5000m, balance.PlannedIncome);
        Assert.Equal(5200m, balance.ActualIncome);
        Assert.Equal(200m, balance.IncomeDifference);
        Assert.Equal(3000m, balance.PlannedExpenses);
        Assert.Equal(2900m, balance.ActualExpenses);
        Assert.Equal(-100m, balance.ExpenseDifference);
        Assert.Equal(500m, balance.PlannedSavings);
        Assert.Equal(600m, balance.ActualSavings);
        Assert.Equal(100m, balance.SavingsDifference);
        Assert.Equal(1500m, balance.PlannedResult);
        Assert.Equal(1700m, balance.ActualResult);
        Assert.Equal(200m, balance.ResultDifference);
    }

    [Fact]
    public async Task BudgetBalanceReadRepository_ReturnsPlanOnlyBalance_WhenBudgetDoesNotExist()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var serviceProvider = CreateServiceProvider(connection, _ => { });

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var ownerId = new OwnerId(Guid.NewGuid());
        var period = new BudgetPeriod(2026, 7);
        var budgetPlan = CreateBudgetPlan(ownerId, period);
        var incomeCategory = CreateCategory(ownerId, BudgetCategoryType.Income);
        budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            incomeCategory,
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10));

        dbContext.BudgetCategories.Add(incomeCategory);
        dbContext.BudgetPlans.Add(budgetPlan);
        await dbContext.SaveChangesAsync();

        var repository = scope.ServiceProvider.GetRequiredService<IBudgetBalanceReadRepository>();

        var balance = await repository.GetByOwnerIdAndPeriodAsync(ownerId, period);

        Assert.NotNull(balance);
        Assert.Equal(budgetPlan.Id.Value, balance.BudgetPlanId);
        Assert.Null(balance.BudgetId);
        Assert.Null(balance.BudgetStatus);
        Assert.Equal(5000m, balance.PlannedIncome);
        Assert.Equal(0m, balance.ActualIncome);
        Assert.Equal(-5000m, balance.IncomeDifference);
        Assert.Equal(5000m, balance.PlannedResult);
        Assert.Equal(0m, balance.ActualResult);
        Assert.Equal(-5000m, balance.ResultDifference);
    }

    [Fact]
    public async Task BudgetBalanceReadRepository_ReturnsHistoryBeforeCurrentPeriod()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var serviceProvider = CreateServiceProvider(connection, _ => { });

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var ownerId = new OwnerId(Guid.NewGuid());
        dbContext.BudgetPlans.AddRange(
            CreateBudgetPlan(ownerId, new BudgetPeriod(2026, 5)),
            CreateBudgetPlan(ownerId, new BudgetPeriod(2026, 6)),
            CreateBudgetPlan(ownerId, new BudgetPeriod(2026, 7)),
            CreateBudgetPlan(ownerId, new BudgetPeriod(2026, 8)));
        await dbContext.SaveChangesAsync();

        var repository = scope.ServiceProvider.GetRequiredService<IBudgetBalanceReadRepository>();

        var history = await repository.GetHistoryAsync(
            ownerId,
            new BudgetPeriod(2026, 7),
            year: 2026,
            limit: 2);

        Assert.Collection(
            history,
            first =>
            {
                Assert.Equal(2026, first.Year);
                Assert.Equal(6, first.Month);
            },
            second =>
            {
                Assert.Equal(2026, second.Year);
                Assert.Equal(5, second.Month);
            });
    }

    [Fact]
    public async Task SaveChangesAsync_RejectsDuplicateBudgetPlansForOwnerAndPeriod()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var dbContext = new HomeBudgetDbContext(
            CreateOptions(connection),
            new RecordingDomainEventDispatcher());
        await dbContext.Database.EnsureCreatedAsync();

        var ownerId = new OwnerId(Guid.NewGuid());
        dbContext.BudgetPlans.AddRange(
            CreateBudgetPlan(ownerId, new BudgetPeriod(2026, 7)),
            CreateBudgetPlan(ownerId, new BudgetPeriod(2026, 7)));

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChangesAsync_RejectsDuplicateBudgetsForOwnerAndPeriod()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var dbContext = new HomeBudgetDbContext(
            CreateOptions(connection),
            new RecordingDomainEventDispatcher());
        await dbContext.Database.EnsureCreatedAsync();

        var ownerId = new OwnerId(Guid.NewGuid());
        var period = new BudgetPeriod(2026, 7);
        dbContext.Budgets.AddRange(
            new Budget(
                new BudgetId(Guid.NewGuid()),
                ownerId,
                period,
                Currency.PLN,
                new BudgetPlanId(Guid.NewGuid())),
            new Budget(
                new BudgetId(Guid.NewGuid()),
                ownerId,
                period,
                Currency.PLN,
                new BudgetPlanId(Guid.NewGuid())));

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
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

    private static BudgetPlan CreateBudgetPlan(OwnerId ownerId, BudgetPeriod? period = null)
        => new(
            new BudgetPlanId(Guid.NewGuid()),
            ownerId,
            period ?? new BudgetPeriod(2026, 7),
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
