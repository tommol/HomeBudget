using HomeBudget.Contracts.Planning;
using HomeBudget.Domain.Shared;
using HomeBudget.Infrastructure.Server.Identity;
using HomeBudget.Infrastructure.Server.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace HomeBudget.Api.Tests;

public sealed class PlanningEndpointsTests
{
    [Fact]
    public async Task CreateBudgetPlan_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var factory = new HomeBudgetApiFactory();
        var client = factory.CreateHttpsClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/planning/budget-plans",
            new CreateBudgetPlanRequest(2026, 7, "PLN"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateBudgetPlan_WithAuthenticatedUserWithoutAccount_ReturnsForbidden()
    {
        using var factory = new HomeBudgetApiFactory();
        await factory.EnsureDatabaseCreatedAsync();
        var client = factory.CreateAuthenticatedClient("missing-account");

        var response = await client.PostAsJsonAsync(
            "/api/v1/planning/budget-plans",
            new CreateBudgetPlanRequest(2026, 7, "PLN"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateBudgetPlan_WithPreProvisionedAccount_CreatesBudgetPlanForMappedOwner()
    {
        var ownerId = Guid.NewGuid();
        using var factory = new HomeBudgetApiFactory();
        await factory.SeedUserAccountAsync("known-account", ownerId);
        var client = factory.CreateAuthenticatedClient("known-account");

        var response = await client.PostAsJsonAsync(
            "/api/v1/planning/budget-plans",
            new CreateBudgetPlanRequest(2026, 7, "PLN"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CreateBudgetPlanResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
        var budgetPlan = await dbContext.BudgetPlans.SingleAsync();

        Assert.Equal(body.Id, budgetPlan.Id.Value);
        Assert.Equal(ownerId, budgetPlan.OwnerId.Value);
    }

    [Theory]
    [MemberData(nameof(InvalidCreateBudgetPlanRequests))]
    public async Task CreateBudgetPlan_WithInvalidRequest_ReturnsBadRequest(CreateBudgetPlanRequest request)
    {
        using var factory = new HomeBudgetApiFactory();
        await factory.SeedUserAccountAsync("known-account", Guid.NewGuid());
        var client = factory.CreateAuthenticatedClient("known-account");

        var response = await client.PostAsJsonAsync("/api/v1/planning/budget-plans", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(StatusCodes.Status400BadRequest, problem?.Status);
    }

    [Fact]
    public async Task OpenApi_InDevelopment_IncludesV1EndpointAndBearerScheme()
    {
        using var factory = new HomeBudgetApiFactory();
        var client = factory.CreateHttpsClient();

        var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var document = await response.Content.ReadAsStringAsync();
        Assert.Contains("/api/v1/planning/budget-plans", document, StringComparison.Ordinal);
        Assert.Contains("\"Bearer\"", document, StringComparison.Ordinal);
        Assert.Contains("\"bearer\"", document, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Scalar_InDevelopment_ReturnsApiReference()
    {
        using var factory = new HomeBudgetApiFactory();
        var client = factory.CreateHttpsClient();

        var response = await client.GetAsync("/scalar/v1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    public static TheoryData<CreateBudgetPlanRequest> InvalidCreateBudgetPlanRequests()
        => new()
        {
            new CreateBudgetPlanRequest(2026, 13, "PLN"),
            new CreateBudgetPlanRequest(2026, 7, "PLNN")
        };

    private sealed class HomeBudgetApiFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        private readonly Dictionary<string, string?> _previousEnvironmentVariables = [];

        public HomeBudgetApiFactory()
        {
            SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            SetEnvironmentVariable("ConnectionStrings__HomeBudget", "Host=localhost;Database=homebudget_tests;Username=test;Password=test");
            SetEnvironmentVariable("Authentication__Authority", TestAuthenticationHandler.Issuer);
            SetEnvironmentVariable("Authentication__Audience", "homebudget-api");

            _connection.Open();
        }

        public HttpClient CreateHttpsClient()
            => CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        public HttpClient CreateAuthenticatedClient(string subject)
        {
            var client = CreateHttpsClient();
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.SubjectHeaderName, subject);

            return client;
        }

        public async Task EnsureDatabaseCreatedAsync()
        {
            await using var scope = Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();

            await dbContext.Database.EnsureCreatedAsync();
        }

        public async Task SeedUserAccountAsync(string subject, Guid ownerId)
        {
            await using var scope = Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();

            await dbContext.Database.EnsureCreatedAsync();
            dbContext.UserAccounts.Add(new UserAccount(
                Guid.NewGuid(),
                new OwnerId(ownerId),
                TestAuthenticationHandler.Issuer,
                subject));
            await dbContext.SaveChangesAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<HomeBudgetDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<HomeBudgetDbContext>>();
                services.RemoveAll<HomeBudgetDbContext>();
                services.RemoveAll<IDatabaseProvider>();
                services.AddDbContext<HomeBudgetDbContext>(options => options.UseSqlite(_connection));

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
                    options.DefaultForbidScheme = TestAuthenticationHandler.SchemeName;
                }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName,
                    _ => { });

                services.PostConfigure<AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
                    options.DefaultForbidScheme = TestAuthenticationHandler.SchemeName;
                });
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _connection.Dispose();

                foreach (var (name, value) in _previousEnvironmentVariables)
                {
                    Environment.SetEnvironmentVariable(name, value);
                }
            }
        }

        private void SetEnvironmentVariable(string name, string value)
        {
            _previousEnvironmentVariables[name] = Environment.GetEnvironmentVariable(name);
            Environment.SetEnvironmentVariable(name, value);
        }
    }

    private sealed class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "Test";
        public const string Issuer = "https://issuer.example";
        public const string SubjectHeaderName = "X-Test-Subject";

        public TestAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(SubjectHeaderName, out var subjectValues))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var subject = subjectValues.ToString();

            if (string.IsNullOrWhiteSpace(subject))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new[]
            {
                new Claim("iss", Issuer),
                new Claim("sub", subject)
            };
            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
