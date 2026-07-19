using HomeBudget.Contracts.Execution;
using HomeBudget.Contracts.Planning;
using HomeBudget.Domain.Execution;
using HomeBudget.Domain.Planning;
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

    [Fact]
    public async Task AddPlannedIncome_WithExistingPlanAndCategory_AddsIncome()
    {
        var ownerId = Guid.NewGuid();
        using var factory = new HomeBudgetApiFactory();
        await factory.SeedUserAccountAsync("known-account", ownerId);
        var budgetPlanId = await factory.SeedBudgetPlanAsync(ownerId);
        var categoryId = await factory.SeedBudgetCategoryAsync(ownerId, BudgetCategoryType.Income);
        var client = factory.CreateAuthenticatedClient("known-account");

        var response = await client.PostAsJsonAsync(
            $"/api/v1/planning/budget-plans/{budgetPlanId}/planned-incomes",
            new AddPlannedIncomeRequest(
                categoryId,
                "Salary",
                5000m,
                "PLN",
                new DateOnly(2026, 7, 10)));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AddPlannedIncomeResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
        var budgetPlan = await dbContext.BudgetPlans.SingleAsync();
        var plannedIncome = Assert.Single(budgetPlan.PlannedIncomes);

        Assert.Equal(body.Id, plannedIncome.Id.Value);
        Assert.Equal(categoryId, plannedIncome.CategoryId.Value);
        Assert.Equal("Salary", plannedIncome.Title);
        Assert.Equal(5000m, plannedIncome.Amount.Amount);
    }

    [Fact]
    public async Task AddPlannedIncome_WithMissingCategory_ReturnsNotFound()
    {
        var ownerId = Guid.NewGuid();
        using var factory = new HomeBudgetApiFactory();
        await factory.SeedUserAccountAsync("known-account", ownerId);
        var budgetPlanId = await factory.SeedBudgetPlanAsync(ownerId);
        var client = factory.CreateAuthenticatedClient("known-account");

        var response = await client.PostAsJsonAsync(
            $"/api/v1/planning/budget-plans/{budgetPlanId}/planned-incomes",
            new AddPlannedIncomeRequest(
                Guid.NewGuid(),
                "Salary",
                5000m,
                "PLN",
                new DateOnly(2026, 7, 10)));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ExpenseCategoryAllocationEndpoints_CreateUpdateAndRemoveAllocation()
    {
        var ownerId = Guid.NewGuid();
        using var factory = new HomeBudgetApiFactory();
        await factory.SeedUserAccountAsync("known-account", ownerId);
        var budgetPlanId = await factory.SeedBudgetPlanAsync(ownerId);
        var categoryId = await factory.SeedBudgetCategoryAsync(ownerId, BudgetCategoryType.Expense);
        var client = factory.CreateAuthenticatedClient("known-account");

        var createResponse = await client.PostAsJsonAsync(
            $"/api/v1/planning/budget-plans/{budgetPlanId}/expense-category-allocations",
            new AddExpenseCategoryAllocationRequest(categoryId, 900m, "Flexible"));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createBody = await createResponse.Content.ReadFromJsonAsync<AddExpenseCategoryAllocationResponse>();
        Assert.NotNull(createBody);
        var allocationId = createBody.Id;

        var changeAmountResponse = await client.PatchAsJsonAsync(
            $"/api/v1/planning/budget-plans/{budgetPlanId}/expense-category-allocations/{allocationId}/amount",
            new ChangeExpenseCategoryAllocationAmountRequest(750m));
        var changeFlexibilityResponse = await client.PatchAsJsonAsync(
            $"/api/v1/planning/budget-plans/{budgetPlanId}/expense-category-allocations/{allocationId}/flexibility",
            new ChangeExpenseCategoryAllocationFlexibilityRequest("Optional"));

        Assert.Equal(HttpStatusCode.NoContent, changeAmountResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, changeFlexibilityResponse.StatusCode);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
            var budgetPlan = await dbContext.BudgetPlans.SingleAsync();
            var allocation = Assert.Single(budgetPlan.ExpenseCategoryAllocations);

            Assert.Equal(750m, allocation.Amount.Amount);
            Assert.Equal(CategoryAllocationFlexibility.Optional, allocation.Flexibility);
        }

        var deleteResponse = await client.DeleteAsync(
            $"/api/v1/planning/budget-plans/{budgetPlanId}/expense-category-allocations/{allocationId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDbContext = verificationScope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
        var reloadedBudgetPlan = await verificationDbContext.BudgetPlans.SingleAsync();

        Assert.Empty(reloadedBudgetPlan.ExpenseCategoryAllocations);
    }

    [Fact]
    public async Task SavingContributionEndpoints_CreateUpdateAndRemoveContribution()
    {
        var ownerId = Guid.NewGuid();
        using var factory = new HomeBudgetApiFactory();
        await factory.SeedUserAccountAsync("known-account", ownerId);
        var budgetPlanId = await factory.SeedBudgetPlanAsync(ownerId);
        var categoryId = await factory.SeedBudgetCategoryAsync(ownerId, BudgetCategoryType.Saving);
        var client = factory.CreateAuthenticatedClient("known-account");

        var createResponse = await client.PostAsJsonAsync(
            $"/api/v1/planning/budget-plans/{budgetPlanId}/saving-contributions",
            new AddSavingContributionRequest(categoryId, 1000m));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createBody = await createResponse.Content.ReadFromJsonAsync<AddSavingContributionResponse>();
        Assert.NotNull(createBody);
        var savingContributionId = createBody.Id;

        var changeAmountResponse = await client.PatchAsJsonAsync(
            $"/api/v1/planning/budget-plans/{budgetPlanId}/saving-contributions/{savingContributionId}/amount",
            new ChangeSavingContributionAmountRequest(1200m));

        Assert.Equal(HttpStatusCode.NoContent, changeAmountResponse.StatusCode);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
            var budgetPlan = await dbContext.BudgetPlans.SingleAsync();
            var contribution = Assert.Single(budgetPlan.SavingContributions);

            Assert.Equal(1200m, contribution.Amount.Amount);
        }

        var deleteResponse = await client.DeleteAsync(
            $"/api/v1/planning/budget-plans/{budgetPlanId}/saving-contributions/{savingContributionId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDbContext = verificationScope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
        var reloadedBudgetPlan = await verificationDbContext.BudgetPlans.SingleAsync();

        Assert.Empty(reloadedBudgetPlan.SavingContributions);
    }

    [Fact]
    public async Task StatusEndpoints_ActivateAndCloseBudgetPlan()
    {
        var ownerId = Guid.NewGuid();
        using var factory = new HomeBudgetApiFactory();
        await factory.SeedUserAccountAsync("known-account", ownerId);
        var budgetPlanId = await factory.SeedBudgetPlanAsync(ownerId);
        var client = factory.CreateAuthenticatedClient("known-account");

        var activateResponse = await client.PostAsync(
            $"/api/v1/planning/budget-plans/{budgetPlanId}/activate",
            content: null);
        var closeResponse = await client.PostAsync(
            $"/api/v1/planning/budget-plans/{budgetPlanId}/close",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, activateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, closeResponse.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
        var budgetPlan = await dbContext.BudgetPlans.SingleAsync();

        Assert.Equal(BudgetPlanStatus.Closed, budgetPlan.Status);
    }

    [Fact]
    public async Task CopyBudgetPlan_CopiesSelectedEntriesToTargetPeriod()
    {
        var ownerId = Guid.NewGuid();
        using var factory = new HomeBudgetApiFactory();
        await factory.SeedUserAccountAsync("known-account", ownerId);
        var budgetPlanId = await factory.SeedBudgetPlanAsync(ownerId);
        var incomeCategoryId = await factory.SeedBudgetCategoryAsync(ownerId, BudgetCategoryType.Income);
        var expenseCategoryId = await factory.SeedBudgetCategoryAsync(ownerId, BudgetCategoryType.Expense);
        var savingCategoryId = await factory.SeedBudgetCategoryAsync(ownerId, BudgetCategoryType.Saving);
        var client = factory.CreateAuthenticatedClient("known-account");

        await client.PostAsJsonAsync(
            $"/api/v1/planning/budget-plans/{budgetPlanId}/planned-incomes",
            new AddPlannedIncomeRequest(
                incomeCategoryId,
                "Salary",
                5000m,
                "PLN",
                new DateOnly(2026, 7, 10)));
        await client.PostAsJsonAsync(
            $"/api/v1/planning/budget-plans/{budgetPlanId}/expense-category-allocations",
            new AddExpenseCategoryAllocationRequest(expenseCategoryId, 900m, "Flexible"));
        await client.PostAsJsonAsync(
            $"/api/v1/planning/budget-plans/{budgetPlanId}/saving-contributions",
            new AddSavingContributionRequest(savingCategoryId, 1000m));

        var copyResponse = await client.PostAsJsonAsync(
            $"/api/v1/planning/budget-plans/{budgetPlanId}/copies",
            new CopyBudgetPlanRequest(
                2026,
                8,
                CopyPlannedIncomes: true,
                CopyExpenseCategoryAllocations: false,
                CopySavingContributions: true));
        var copyResponseContent = await copyResponse.Content.ReadAsStringAsync();

        Assert.True(copyResponse.StatusCode == HttpStatusCode.Created, copyResponseContent);

        var copyBody = await copyResponse.Content.ReadFromJsonAsync<CopyBudgetPlanResponse>();
        Assert.NotNull(copyBody);
        Assert.NotEqual(Guid.Empty, copyBody.Id);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
        var copiedBudgetPlan = await dbContext.BudgetPlans.SingleAsync(plan => plan.Id == new BudgetPlanId(copyBody.Id));

        Assert.Equal(2026, copiedBudgetPlan.Period.Year);
        Assert.Equal(8, copiedBudgetPlan.Period.Month);
        Assert.Single(copiedBudgetPlan.PlannedIncomes);
        Assert.Empty(copiedBudgetPlan.ExpenseCategoryAllocations);
        Assert.Single(copiedBudgetPlan.SavingContributions);
    }

    [Fact]
    public async Task IncomeEndpoints_CreateUpdateAndRemoveIncome()
    {
        var ownerId = Guid.NewGuid();
        using var factory = new HomeBudgetApiFactory();
        await factory.SeedUserAccountAsync("known-account", ownerId);
        var budgetId = await factory.SeedBudgetAsync(ownerId);
        var categoryId = await factory.SeedBudgetCategoryAsync(ownerId, BudgetCategoryType.Income);
        var newCategoryId = await factory.SeedBudgetCategoryAsync(ownerId, BudgetCategoryType.Income);
        var client = factory.CreateAuthenticatedClient("known-account");

        var createResponse = await client.PostAsJsonAsync(
            $"/api/v1/execution/budgets/{budgetId}/incomes",
            new AddIncomeRequest(
                categoryId,
                "Salary",
                5000m,
                "PLN",
                new DateOnly(2026, 7, 10)));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createBody = await createResponse.Content.ReadFromJsonAsync<AddIncomeResponse>();
        Assert.NotNull(createBody);
        var incomeId = createBody.Id;

        var changeAmountResponse = await client.PatchAsJsonAsync(
            $"/api/v1/execution/budgets/{budgetId}/incomes/{incomeId}/amount",
            new ChangeIncomeAmountRequest(5500m, "PLN"));
        var changeCategoryResponse = await client.PatchAsJsonAsync(
            $"/api/v1/execution/budgets/{budgetId}/incomes/{incomeId}/category",
            new ChangeIncomeCategoryRequest(newCategoryId));
        var changeTitleResponse = await client.PatchAsJsonAsync(
            $"/api/v1/execution/budgets/{budgetId}/incomes/{incomeId}/title",
            new ChangeIncomeTitleRequest("Updated salary"));
        var changeDateResponse = await client.PatchAsJsonAsync(
            $"/api/v1/execution/budgets/{budgetId}/incomes/{incomeId}/occurred-date",
            new ChangeIncomeOccurredDateRequest(new DateOnly(2026, 7, 12)));

        Assert.Equal(HttpStatusCode.NoContent, changeAmountResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, changeCategoryResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, changeTitleResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, changeDateResponse.StatusCode);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
            var budget = await dbContext.Budgets.SingleAsync();
            var income = Assert.Single(budget.Incomes);

            Assert.Equal(incomeId, income.Id.Value);
            Assert.Equal(newCategoryId, income.CategoryId.Value);
            Assert.Equal("Updated salary", income.Title);
            Assert.Equal(5500m, income.Amount.Amount);
            Assert.Equal(new DateOnly(2026, 7, 12), income.OccurredDate);
        }

        using var deleteRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/v1/execution/budgets/{budgetId}/incomes/{incomeId}")
        {
            Content = JsonContent.Create(new RemoveIncomeRequest("Duplicate"))
        };
        var deleteResponse = await client.SendAsync(deleteRequest);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDbContext = verificationScope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
        var reloadedBudget = await verificationDbContext.Budgets.SingleAsync();
        var removedIncome = Assert.Single(reloadedBudget.Incomes);

        Assert.True(removedIncome.IsRemoved);
        Assert.Equal("Duplicate", removedIncome.RemovalReason);
    }

    [Fact]
    public async Task ExpenseEndpoints_CreateUpdateAndRemoveExpense()
    {
        var ownerId = Guid.NewGuid();
        using var factory = new HomeBudgetApiFactory();
        await factory.SeedUserAccountAsync("known-account", ownerId);
        var budgetId = await factory.SeedBudgetAsync(ownerId);
        var categoryId = await factory.SeedBudgetCategoryAsync(ownerId, BudgetCategoryType.Expense);
        var newCategoryId = await factory.SeedBudgetCategoryAsync(ownerId, BudgetCategoryType.Expense);
        var client = factory.CreateAuthenticatedClient("known-account");

        var createResponse = await client.PostAsJsonAsync(
            $"/api/v1/execution/budgets/{budgetId}/expenses",
            new AddExpenseRequest(
                categoryId,
                "Groceries",
                200m,
                "PLN",
                new DateOnly(2026, 7, 5)));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createBody = await createResponse.Content.ReadFromJsonAsync<AddExpenseResponse>();
        Assert.NotNull(createBody);
        var expenseId = createBody.Id;

        var changeAmountResponse = await client.PatchAsJsonAsync(
            $"/api/v1/execution/budgets/{budgetId}/expenses/{expenseId}/amount",
            new ChangeExpenseAmountRequest(250m, "PLN"));
        var changeCategoryResponse = await client.PatchAsJsonAsync(
            $"/api/v1/execution/budgets/{budgetId}/expenses/{expenseId}/category",
            new ChangeExpenseCategoryRequest(newCategoryId));
        var changeTitleResponse = await client.PatchAsJsonAsync(
            $"/api/v1/execution/budgets/{budgetId}/expenses/{expenseId}/title",
            new ChangeExpenseTitleRequest("Updated groceries"));
        var changeDateResponse = await client.PatchAsJsonAsync(
            $"/api/v1/execution/budgets/{budgetId}/expenses/{expenseId}/occurred-date",
            new ChangeExpenseOccurredDateRequest(new DateOnly(2026, 7, 6)));

        Assert.Equal(HttpStatusCode.NoContent, changeAmountResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, changeCategoryResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, changeTitleResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, changeDateResponse.StatusCode);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
            var budget = await dbContext.Budgets.SingleAsync();
            var expense = Assert.Single(budget.Expenses);

            Assert.Equal(expenseId, expense.Id.Value);
            Assert.Equal(newCategoryId, expense.CategoryId.Value);
            Assert.Equal("Updated groceries", expense.Title);
            Assert.Equal(250m, expense.Amount.Amount);
            Assert.Equal(new DateOnly(2026, 7, 6), expense.OccurredDate);
        }

        using var deleteRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/v1/execution/budgets/{budgetId}/expenses/{expenseId}")
        {
            Content = JsonContent.Create(new RemoveExpenseRequest("Duplicate"))
        };
        var deleteResponse = await client.SendAsync(deleteRequest);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDbContext = verificationScope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
        var reloadedBudget = await verificationDbContext.Budgets.SingleAsync();
        var removedExpense = Assert.Single(reloadedBudget.Expenses);

        Assert.True(removedExpense.IsRemoved);
        Assert.Equal("Duplicate", removedExpense.RemovalReason);
    }

    [Fact]
    public async Task SavingEndpoints_CreateUpdateAndRemoveSaving()
    {
        var ownerId = Guid.NewGuid();
        using var factory = new HomeBudgetApiFactory();
        await factory.SeedUserAccountAsync("known-account", ownerId);
        var budgetId = await factory.SeedBudgetAsync(ownerId);
        var categoryId = await factory.SeedBudgetCategoryAsync(ownerId, BudgetCategoryType.Saving);
        var newCategoryId = await factory.SeedBudgetCategoryAsync(ownerId, BudgetCategoryType.Saving);
        var client = factory.CreateAuthenticatedClient("known-account");

        var createResponse = await client.PostAsJsonAsync(
            $"/api/v1/execution/budgets/{budgetId}/savings",
            new AddSavingRequest(
                categoryId,
                "Emergency fund",
                1000m,
                "PLN",
                new DateOnly(2026, 7, 20)));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createBody = await createResponse.Content.ReadFromJsonAsync<AddSavingResponse>();
        Assert.NotNull(createBody);
        var savingId = createBody.Id;

        var changeAmountResponse = await client.PatchAsJsonAsync(
            $"/api/v1/execution/budgets/{budgetId}/savings/{savingId}/amount",
            new ChangeSavingAmountRequest(1200m, "PLN"));
        var changeCategoryResponse = await client.PatchAsJsonAsync(
            $"/api/v1/execution/budgets/{budgetId}/savings/{savingId}/category",
            new ChangeSavingCategoryRequest(newCategoryId));
        var changeTitleResponse = await client.PatchAsJsonAsync(
            $"/api/v1/execution/budgets/{budgetId}/savings/{savingId}/title",
            new ChangeSavingTitleRequest("Updated emergency fund"));
        var changeDateResponse = await client.PatchAsJsonAsync(
            $"/api/v1/execution/budgets/{budgetId}/savings/{savingId}/occurred-date",
            new ChangeSavingOccurredDateRequest(new DateOnly(2026, 7, 21)));

        Assert.Equal(HttpStatusCode.NoContent, changeAmountResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, changeCategoryResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, changeTitleResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, changeDateResponse.StatusCode);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
            var budget = await dbContext.Budgets.SingleAsync();
            var saving = Assert.Single(budget.Savings);

            Assert.Equal(savingId, saving.Id.Value);
            Assert.Equal(newCategoryId, saving.CategoryId.Value);
            Assert.Equal("Updated emergency fund", saving.Title);
            Assert.Equal(1200m, saving.Amount.Amount);
            Assert.Equal(new DateOnly(2026, 7, 21), saving.OccurredDate);
        }

        using var deleteRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/v1/execution/budgets/{budgetId}/savings/{savingId}")
        {
            Content = JsonContent.Create(new RemoveSavingRequest("Duplicate"))
        };
        var deleteResponse = await client.SendAsync(deleteRequest);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDbContext = verificationScope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
        var reloadedBudget = await verificationDbContext.Budgets.SingleAsync();
        var removedSaving = Assert.Single(reloadedBudget.Savings);

        Assert.True(removedSaving.IsRemoved);
        Assert.Equal("Duplicate", removedSaving.RemovalReason);
    }

    [Fact]
    public async Task CloseBudget_WithExistingBudget_ClosesBudget()
    {
        var ownerId = Guid.NewGuid();
        using var factory = new HomeBudgetApiFactory();
        await factory.SeedUserAccountAsync("known-account", ownerId);
        var budgetId = await factory.SeedBudgetAsync(ownerId);
        var client = factory.CreateAuthenticatedClient("known-account");

        var response = await client.PostAsync(
            $"/api/v1/execution/budgets/{budgetId}/close",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
        var budget = await dbContext.Budgets.SingleAsync();

        Assert.Equal(BudgetStatus.Closed, budget.Status);
    }

    [Fact]
    public async Task AddIncome_WithMissingBudget_ReturnsNotFound()
    {
        var ownerId = Guid.NewGuid();
        using var factory = new HomeBudgetApiFactory();
        await factory.SeedUserAccountAsync("known-account", ownerId);
        var categoryId = await factory.SeedBudgetCategoryAsync(ownerId, BudgetCategoryType.Income);
        var client = factory.CreateAuthenticatedClient("known-account");

        var response = await client.PostAsJsonAsync(
            $"/api/v1/execution/budgets/{Guid.NewGuid()}/incomes",
            new AddIncomeRequest(
                categoryId,
                "Salary",
                5000m,
                "PLN",
                new DateOnly(2026, 7, 10)));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
        Assert.Contains("/api/v1/planning/budget-plans/{budgetPlanId}/planned-incomes", document, StringComparison.Ordinal);
        Assert.Contains("/api/v1/planning/budget-plans/{budgetPlanId}/saving-contributions", document, StringComparison.Ordinal);
        Assert.Contains("/api/v1/execution/budgets/{budgetId}/incomes", document, StringComparison.Ordinal);
        Assert.Contains("/api/v1/execution/budgets/{budgetId}/expenses", document, StringComparison.Ordinal);
        Assert.Contains("/api/v1/execution/budgets/{budgetId}/savings", document, StringComparison.Ordinal);
        Assert.Contains("/api/v1/execution/budgets/{budgetId}/close", document, StringComparison.Ordinal);
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

        public async Task<Guid> SeedBudgetPlanAsync(
            Guid ownerId,
            int year = 2026,
            int month = 7,
            string currencyCode = "PLN")
        {
            await using var scope = Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
            var budgetPlan = new BudgetPlan(
                new BudgetPlanId(Guid.NewGuid()),
                new OwnerId(ownerId),
                new BudgetPeriod(year, month),
                new Currency(currencyCode));

            await dbContext.Database.EnsureCreatedAsync();
            dbContext.BudgetPlans.Add(budgetPlan);
            await dbContext.SaveChangesAsync();

            return budgetPlan.Id.Value;
        }

        public async Task<Guid> SeedBudgetAsync(
            Guid ownerId,
            int year = 2026,
            int month = 7,
            string currencyCode = "PLN")
        {
            await using var scope = Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
            var budget = new Budget(
                new BudgetId(Guid.NewGuid()),
                new OwnerId(ownerId),
                new BudgetPeriod(year, month),
                new Currency(currencyCode));

            await dbContext.Database.EnsureCreatedAsync();
            dbContext.Budgets.Add(budget);
            await dbContext.SaveChangesAsync();

            return budget.Id.Value;
        }

        public async Task<Guid> SeedBudgetCategoryAsync(Guid ownerId, BudgetCategoryType type)
        {
            await using var scope = Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<HomeBudgetDbContext>();
            var budgetCategory = new BudgetCategory(
                new BudgetCategoryId(Guid.NewGuid()),
                new OwnerId(ownerId),
                $"{type} Category",
                type);

            await dbContext.Database.EnsureCreatedAsync();
            dbContext.BudgetCategories.Add(budgetCategory);
            await dbContext.SaveChangesAsync();

            return budgetCategory.Id.Value;
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
