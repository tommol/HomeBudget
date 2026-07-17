using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Planning;
using HomeBudget.Infrastructure.Server.Identity;
using HomeBudget.Infrastructure.Server.DomainEvents;
using HomeBudget.Infrastructure.Server.Persistence;
using HomeBudget.Infrastructure.Server.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeBudget.Infrastructure.Server;

/// <summary>
/// Registers server-side infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers server-side infrastructure services using the configured PostgreSQL connection string.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddServerInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("HomeBudget")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'HomeBudget' or 'DefaultConnection' is required.");

        return services.AddServerInfrastructure(options => options.UseNpgsql(connectionString));
    }

    /// <summary>
    /// Registers server-side infrastructure services using custom EF Core options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureDbContext">The EF Core options configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddServerInfrastructure(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDbContext);

        services.AddDbContext<HomeBudgetDbContext>(configureDbContext);
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<HomeBudgetDbContext>());
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IUserAccountRepository, EfUserAccountRepository>();
        services.AddScoped<IBudgetPlanRepository, EfBudgetPlanRepository>();
        services.AddScoped<IBudgetCategoryRepository, EfBudgetCategoryRepository>();

        return services;
    }
}
