using Asp.Versioning;
using HomeBudget.Api.Auth;
using HomeBudget.Api.Endpoints.Planning;
using HomeBudget.Api.OpenApi;
using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Planning.ActivateBudgetPlan;
using HomeBudget.Application.Planning.AddExpenseCategoryAllocation;
using HomeBudget.Application.Planning.AddPlannedIncome;
using HomeBudget.Application.Planning.AddSavingContribution;
using HomeBudget.Application.Planning.ChangeExpenseCategoryAllocationAmount;
using HomeBudget.Application.Planning.ChangeExpenseCategoryAllocationFlexibility;
using HomeBudget.Application.Planning.ChangeSavingContributionAmount;
using HomeBudget.Application.Planning.CloseBudgetPlan;
using HomeBudget.Application.Planning.CopyBudgetPlan;
using HomeBudget.Application.Planning.CreateBudgetPlan;
using HomeBudget.Application.Planning.RemoveExpenseCategoryAllocation;
using HomeBudget.Application.Planning.RemoveSavingContribution;
using HomeBudget.Infrastructure.Server;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServerInfrastructure(builder.Configuration);
builder.Services.AddScoped<ICommandHandler<ActivateBudgetPlanCommand>, ActivateBudgetPlanCommandHandler>();
builder.Services.AddScoped<ICommandHandler<AddExpenseCategoryAllocationCommand, Guid>, AddExpenseCategoryAllocationCommandHandler>();
builder.Services.AddScoped<ICommandHandler<AddPlannedIncomeCommand, Guid>, AddPlannedIncomeCommandHandler>();
builder.Services.AddScoped<ICommandHandler<AddSavingContributionCommand, Guid>, AddSavingContributionCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ChangeExpenseCategoryAllocationAmountCommand>, ChangeExpenseCategoryAllocationAmountCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ChangeExpenseCategoryAllocationFlexibilityCommand>, ChangeExpenseCategoryAllocationFlexibilityCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ChangeSavingContributionAmountCommand>, ChangeSavingContributionAmountCommandHandler>();
builder.Services.AddScoped<ICommandHandler<CloseBudgetPlanCommand>, CloseBudgetPlanCommandHandler>();
builder.Services.AddScoped<ICommandHandler<CopyBudgetPlanCommand, Guid>, CopyBudgetPlanCommandHandler>();
builder.Services.AddScoped<ICommandHandler<CreateBudgetPlanCommand, Guid>, CreateBudgetPlanCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RemoveExpenseCategoryAllocationCommand>, RemoveExpenseCategoryAllocationCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RemoveSavingContributionCommand>, RemoveSavingContributionCommandHandler>();

builder.Services.AddScoped<CurrentOwnerContext>();
builder.Services.AddScoped<ICurrentOwner>(serviceProvider => serviceProvider.GetRequiredService<CurrentOwnerContext>());
builder.Services.AddScoped<CurrentOwnerEndpointFilter>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"]
            ?? throw new InvalidOperationException("Authentication:Authority is required.");
        options.Audience = builder.Configuration["Authentication:Audience"]
            ?? throw new InvalidOperationException("Authentication:Audience is required.");
        options.MapInboundClaims = false;
    });

builder.Services.AddAuthorization();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer<ApiVersionPathDocumentTransformer>();
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference(options =>
    {
        options.AddDocument("v1", "HomeBudget API v1");
    }).AllowAnonymous();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok())
    .AllowAnonymous();

var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

var api = app.MapGroup("/api/v{version:apiVersion}")
    .WithApiVersionSet(apiVersionSet)
    .WithGroupName("v1")
    .RequireAuthorization();

api.MapPlanningEndpoints();

app.Run();

/// <summary>
/// Exposes the top-level program type to integration tests.
/// </summary>
public partial class Program;
