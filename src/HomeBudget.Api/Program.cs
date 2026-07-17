using Asp.Versioning;
using HomeBudget.Api.Auth;
using HomeBudget.Api.Endpoints.Planning;
using HomeBudget.Api.OpenApi;
using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Execution.AddExpense;
using HomeBudget.Application.Execution.AddIncome;
using HomeBudget.Application.Execution.AddSaving;
using HomeBudget.Application.Execution.ChangeExpenseAmount;
using HomeBudget.Application.Execution.ChangeExpenseCategory;
using HomeBudget.Application.Execution.ChangeExpenseOccurredDate;
using HomeBudget.Application.Execution.ChangeExpenseTitle;
using HomeBudget.Application.Execution.ChangeIncomeAmount;
using HomeBudget.Application.Execution.ChangeIncomeCategory;
using HomeBudget.Application.Execution.ChangeIncomeOccurredDate;
using HomeBudget.Application.Execution.ChangeIncomeTitle;
using HomeBudget.Application.Execution.ChangeSavingAmount;
using HomeBudget.Application.Execution.ChangeSavingCategory;
using HomeBudget.Application.Execution.ChangeSavingOccurredDate;
using HomeBudget.Application.Execution.ChangeSavingTitle;
using HomeBudget.Application.Execution.CloseBudget;
using HomeBudget.Application.Execution.RemoveExpense;
using HomeBudget.Application.Execution.RemoveIncome;
using HomeBudget.Application.Execution.RemoveSaving;
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
builder.Services.AddScoped<ICommandHandler<AddExpenseCommand, Guid>, AddExpenseCommandHandler>();
builder.Services.AddScoped<ICommandHandler<AddIncomeCommand, Guid>, AddIncomeCommandHandler>();
builder.Services.AddScoped<ICommandHandler<AddSavingCommand, Guid>, AddSavingCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ChangeExpenseAmountCommand>, ChangeExpenseAmountCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ChangeExpenseCategoryCommand>, ChangeExpenseCategoryCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ChangeExpenseOccurredDateCommand>, ChangeExpenseOccurredDateCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ChangeExpenseTitleCommand>, ChangeExpenseTitleCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ChangeIncomeAmountCommand>, ChangeIncomeAmountCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ChangeIncomeCategoryCommand>, ChangeIncomeCategoryCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ChangeIncomeOccurredDateCommand>, ChangeIncomeOccurredDateCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ChangeIncomeTitleCommand>, ChangeIncomeTitleCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ChangeSavingAmountCommand>, ChangeSavingAmountCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ChangeSavingCategoryCommand>, ChangeSavingCategoryCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ChangeSavingOccurredDateCommand>, ChangeSavingOccurredDateCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ChangeSavingTitleCommand>, ChangeSavingTitleCommandHandler>();
builder.Services.AddScoped<ICommandHandler<CloseBudgetCommand>, CloseBudgetCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RemoveExpenseCommand>, RemoveExpenseCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RemoveIncomeCommand>, RemoveIncomeCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RemoveSavingCommand>, RemoveSavingCommandHandler>();
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
