using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Kernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace HomeBudget.Infrastructure.Server.Persistence;

/// <summary>
/// Creates <see cref="HomeBudgetDbContext"/> instances for EF Core design-time tooling.
/// </summary>
public sealed class HomeBudgetDbContextFactory : IDesignTimeDbContextFactory<HomeBudgetDbContext>
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=homebudget;Username=postgres;Password=postgres";

    /// <inheritdoc />
    public HomeBudgetDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__HomeBudget")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings:HomeBudget")
            ?? GetConnectionStringFromApiSettings()
            ?? DefaultConnectionString;

        var options = new DbContextOptionsBuilder<HomeBudgetDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new HomeBudgetDbContext(options, new NoOpDomainEventDispatcher());
    }

    private static string? GetConnectionStringFromApiSettings()
    {
        var apiDirectory = FindApiDirectory();

        if (apiDirectory is null)
        {
            return null;
        }

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        return ReadConnectionString(Path.Combine(apiDirectory.FullName, $"appsettings.{environment}.json"))
            ?? ReadConnectionString(Path.Combine(apiDirectory.FullName, "appsettings.json"));
    }

    private static DirectoryInfo? FindApiDirectory()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (current is not null)
        {
            var apiDirectory = Path.Combine(current.FullName, "src", "HomeBudget.Api");

            if (Directory.Exists(apiDirectory))
            {
                return new DirectoryInfo(apiDirectory);
            }

            current = current.Parent;
        }

        return null;
    }

    private static string? ReadConnectionString(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(path));

        if (!document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings))
        {
            return null;
        }

        return TryGetConnectionString(connectionStrings, "HomeBudget")
            ?? TryGetConnectionString(connectionStrings, "DefaultConnection");
    }

    private static string? TryGetConnectionString(JsonElement connectionStrings, string name)
        => connectionStrings.TryGetProperty(name, out var value)
            ? value.GetString()
            : null;

    private sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
