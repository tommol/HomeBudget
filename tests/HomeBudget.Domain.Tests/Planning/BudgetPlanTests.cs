using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Tests.Planning;

public sealed class BudgetPlanTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var id = new BudgetPlanId(Guid.NewGuid());
        var ownerId = new OwnerId(Guid.NewGuid());
        var period = new BudgetPeriod(2026, 7);
        var defaultCurrency = Currency.PLN;

        var budgetPlan = new BudgetPlan(id, ownerId, period, defaultCurrency);

        Assert.Equal(id, budgetPlan.Id);
        Assert.Equal(ownerId, budgetPlan.OwnerId);
        Assert.Equal(period, budgetPlan.Period);
        Assert.Equal(defaultCurrency, budgetPlan.DefaultCurrency);
        Assert.Equal(BudgetPlanStatus.Draft, budgetPlan.Status);
        Assert.Equal(BudgetFitRisk.Balanced, budgetPlan.BudgetFitRisk);
        Assert.Equal(Money.Zero(defaultCurrency), budgetPlan.TotalPlannedIncome);
        Assert.Equal(Money.Zero(defaultCurrency), budgetPlan.TotalAllocatedExpenses);
        Assert.Equal(Money.Zero(defaultCurrency), budgetPlan.TotalSavingContributions);
        Assert.Equal(Money.Zero(defaultCurrency), budgetPlan.PlannedFinancialResult);
        Assert.Empty(budgetPlan.PlannedIncomes);
        Assert.Empty(budgetPlan.ExpenseCategoryAllocations);
        Assert.Empty(budgetPlan.SavingContributions);
    }

    [Fact]
    public void AddPlannedIncome_AddsPlannedIncome()
    {
        var budgetPlan = CreateBudgetPlan();
        var plannedIncomeId = new PlannedIncomeId(Guid.NewGuid());
        var category = CreateIncomeCategory(budgetPlan.OwnerId);
        var title = "Salary";
        var amount = new Money(5000m, Currency.PLN);
        var expectedDate = new DateOnly(2026, 7, 10);

        var plannedIncome = budgetPlan.AddPlannedIncome(plannedIncomeId, category, title, amount, expectedDate);

        Assert.Equal(plannedIncomeId, plannedIncome.Id);
        Assert.Equal(category.Id, plannedIncome.CategoryId);
        Assert.Equal(title, plannedIncome.Title);
        Assert.Equal(amount, plannedIncome.Amount);
        Assert.Equal(expectedDate, plannedIncome.ExpectedDate);
        Assert.Null(plannedIncome.ConvertedAmount);
        Assert.Null(plannedIncome.ConversionDate);
        Assert.Contains(plannedIncome, budgetPlan.PlannedIncomes);
        Assert.Equal(new Money(5000m, Currency.PLN), budgetPlan.TotalPlannedIncome);
        Assert.Equal(Money.Zero(Currency.PLN), budgetPlan.TotalAllocatedExpenses);
        Assert.Equal(new Money(5000m, Currency.PLN), budgetPlan.PlannedFinancialResult);
    }

    [Fact]
    public void AddPlannedIncome_AddsConvertedAmount()
    {
        var budgetPlan = CreateBudgetPlan();
        var amount = new Money(1000m, Currency.EUR);
        var convertedAmount = new Money(4250m, Currency.PLN);
        var conversionDate = new DateOnly(2026, 7, 9);

        var plannedIncome = budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budgetPlan.OwnerId),
            "Salary",
            amount,
            new DateOnly(2026, 7, 10),
            convertedAmount,
            conversionDate);

        Assert.Equal(amount, plannedIncome.Amount);
        Assert.Equal(convertedAmount, plannedIncome.ConvertedAmount);
        Assert.Equal(conversionDate, plannedIncome.ConversionDate);
    }

    [Fact]
    public void AddPlannedIncome_RaisesPlannedIncomeAddedEvent()
    {
        var budgetPlan = CreateBudgetPlan();
        var plannedIncomeId = new PlannedIncomeId(Guid.NewGuid());
        var category = CreateIncomeCategory(budgetPlan.OwnerId);
        var title = " Salary ";
        var amount = new Money(1000m, Currency.EUR);
        var convertedAmount = new Money(4250m, Currency.PLN);
        var expectedDate = new DateOnly(2026, 7, 10);
        var conversionDate = new DateOnly(2026, 7, 9);
        var before = DateTimeOffset.UtcNow;

        budgetPlan.AddPlannedIncome(
            plannedIncomeId,
            category,
            title,
            amount,
            expectedDate,
            convertedAmount,
            conversionDate);

        var after = DateTimeOffset.UtcNow;
        var domainEvent = Assert.IsType<PlannedIncomeAddedEvent>(Assert.Single(budgetPlan.DomainEvents));
        Assert.Equal(budgetPlan.Id, domainEvent.BudgetPlanId);
        Assert.Equal(plannedIncomeId, domainEvent.PlannedIncomeId);
        Assert.Equal(category.Id, domainEvent.CategoryId);
        Assert.Equal("Salary", domainEvent.Title);
        Assert.Equal(amount, domainEvent.Amount);
        Assert.Equal(convertedAmount, domainEvent.ConvertedAmount);
        Assert.Equal(conversionDate, domainEvent.ConversionDate);
        Assert.Equal(expectedDate, domainEvent.ExpectedDate);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void AddPlannedIncome_RecalculatesAllocationIncomeShares()
    {
        var budgetPlan = CreateBudgetPlan();
        var allocation = AddAllocation(budgetPlan, 250m, CategoryAllocationFlexibility.Fixed);

        budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budgetPlan.OwnerId),
            "Salary",
            new Money(1000m, Currency.PLN),
            new DateOnly(2026, 7, 10));

        Assert.Equal(25m, allocation.IncomeSharePercentage);
        Assert.Equal(BudgetFitRisk.Balanced, budgetPlan.BudgetFitRisk);
    }

    [Fact]
    public void AddPlannedIncome_Throws_WhenCategoryIsNull()
    {
        var budgetPlan = CreateBudgetPlan();

        Assert.Throws<ArgumentNullException>(() => budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            null!,
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10)));
    }

    [Theory]
    [InlineData(BudgetCategoryType.Expense)]
    [InlineData(BudgetCategoryType.Saving)]
    public void AddPlannedIncome_Throws_WhenCategoryIsNotIncomeCategory(BudgetCategoryType categoryType)
    {
        var budgetPlan = CreateBudgetPlan();
        var category = CreateCategory(budgetPlan.OwnerId, categoryType);

        Assert.Throws<ArgumentException>(() => budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            category,
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10)));
    }

    [Fact]
    public void AddPlannedIncome_Throws_WhenCategoryBelongsToDifferentOwner()
    {
        var budgetPlan = CreateBudgetPlan();
        var category = CreateIncomeCategory(new OwnerId(Guid.NewGuid()));

        Assert.Throws<InvalidOperationException>(() => budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            category,
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10)));
    }

    [Fact]
    public void AddPlannedIncome_Throws_WhenCategoryIsArchived()
    {
        var budgetPlan = CreateBudgetPlan();
        var category = CreateIncomeCategory(budgetPlan.OwnerId);
        category.Archive();

        Assert.Throws<InvalidOperationException>(() => budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            category,
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddPlannedIncome_Throws_WhenTitleIsEmpty(string? title)
    {
        var budgetPlan = CreateBudgetPlan();

        Assert.Throws<ArgumentException>(() => budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budgetPlan.OwnerId),
            title!,
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10)));
    }

    [Fact]
    public void AddPlannedIncome_Throws_WhenTitleIsTooLong()
    {
        var budgetPlan = CreateBudgetPlan();

        Assert.Throws<ArgumentException>(() => budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budgetPlan.OwnerId),
            new string('a', 101),
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10)));
    }

    [Fact]
    public void AddPlannedIncome_TrimsTitle()
    {
        var budgetPlan = CreateBudgetPlan();

        var plannedIncome = budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budgetPlan.OwnerId),
            " Salary ",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10));

        Assert.Equal("Salary", plannedIncome.Title);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddPlannedIncome_Throws_WhenAmountIsNotPositive(decimal amount)
    {
        var budgetPlan = CreateBudgetPlan();

        Assert.Throws<ArgumentOutOfRangeException>(() => budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budgetPlan.OwnerId),
            "Salary",
            new Money(amount, Currency.PLN),
            new DateOnly(2026, 7, 10)));
    }

    [Fact]
    public void AddPlannedIncome_Throws_WhenForeignCurrencyIncomeHasNoConvertedAmount()
    {
        var budgetPlan = CreateBudgetPlan();

        Assert.Throws<ArgumentException>(() => budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budgetPlan.OwnerId),
            "Salary",
            new Money(1000m, Currency.EUR),
            new DateOnly(2026, 7, 10)));
    }

    [Fact]
    public void AddPlannedIncome_Throws_WhenForeignCurrencyIncomeHasNoConversionDate()
    {
        var budgetPlan = CreateBudgetPlan();

        Assert.Throws<ArgumentException>(() => budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budgetPlan.OwnerId),
            "Salary",
            new Money(1000m, Currency.EUR),
            new DateOnly(2026, 7, 10),
            new Money(4250m, Currency.PLN)));
    }

    [Fact]
    public void AddPlannedIncome_Throws_WhenConvertedAmountCurrencyDiffersFromDefaultCurrency()
    {
        var budgetPlan = CreateBudgetPlan();

        Assert.Throws<ArgumentException>(() => budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budgetPlan.OwnerId),
            "Salary",
            new Money(5000m, Currency.EUR),
            new DateOnly(2026, 7, 10),
            new Money(5000m, Currency.EUR),
            new DateOnly(2026, 7, 10)));
    }

    [Fact]
    public void AddPlannedIncome_Throws_WhenConvertedAmountIsNotPositive()
    {
        var budgetPlan = CreateBudgetPlan();

        Assert.Throws<ArgumentOutOfRangeException>(() => budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budgetPlan.OwnerId),
            "Salary",
            new Money(1000m, Currency.EUR),
            new DateOnly(2026, 7, 10),
            new Money(0m, Currency.PLN),
            new DateOnly(2026, 7, 10)));
    }

    [Fact]
    public void AddPlannedIncome_Throws_WhenDefaultCurrencyIncomeHasConvertedAmount()
    {
        var budgetPlan = CreateBudgetPlan();

        Assert.Throws<ArgumentException>(() => budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budgetPlan.OwnerId),
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10),
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10)));
    }

    [Fact]
    public void AddPlannedIncome_Throws_WhenDefaultCurrencyIncomeHasConversionDate()
    {
        var budgetPlan = CreateBudgetPlan();

        Assert.Throws<ArgumentException>(() => budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budgetPlan.OwnerId),
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10),
            conversionDate: new DateOnly(2026, 7, 10)));
    }

    [Theory]
    [InlineData(2026, 6, 30)]
    [InlineData(2026, 8, 1)]
    public void AddPlannedIncome_Throws_WhenExpectedDateIsOutsideBudgetPeriod(int year, int month, int day)
    {
        var budgetPlan = CreateBudgetPlan();

        Assert.Throws<ArgumentOutOfRangeException>(() => budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budgetPlan.OwnerId),
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(year, month, day)));
    }

    [Fact]
    public void AddExpenseCategoryAllocation_AddsCategoryAllocation()
    {
        var budgetPlan = CreateBudgetPlan();
        var allocationId = new CategoryAllocationId(Guid.NewGuid());
        var category = CreateExpenseCategory(budgetPlan.OwnerId);
        var amount = new Money(2500m, Currency.PLN);

        var allocation = budgetPlan.AddExpenseCategoryAllocation(
            allocationId,
            category,
            amount,
            CategoryAllocationFlexibility.Fixed);

        Assert.Equal(allocationId, allocation.Id);
        Assert.Equal(category.Id, allocation.CategoryId);
        Assert.Equal(amount, allocation.Amount);
        Assert.Equal(CategoryAllocationFlexibility.Fixed, allocation.Flexibility);
        Assert.Equal(100m, allocation.ExpenseSharePercentage);
        Assert.Equal(0m, allocation.IncomeSharePercentage);
        Assert.Contains(allocation, budgetPlan.ExpenseCategoryAllocations);
        Assert.Equal(Money.Zero(Currency.PLN), budgetPlan.TotalPlannedIncome);
        Assert.Equal(new Money(2500m, Currency.PLN), budgetPlan.TotalAllocatedExpenses);
        Assert.Equal(new Money(-2500m, Currency.PLN), budgetPlan.PlannedFinancialResult);
    }

    [Fact]
    public void AddExpenseCategoryAllocation_RaisesCategoryAllocationAddedEvent()
    {
        var budgetPlan = CreateBudgetPlan();
        var allocationId = new CategoryAllocationId(Guid.NewGuid());
        var category = CreateExpenseCategory(budgetPlan.OwnerId);
        var amount = new Money(2500m, Currency.PLN);
        var before = DateTimeOffset.UtcNow;

        budgetPlan.AddExpenseCategoryAllocation(
            allocationId,
            category,
            amount,
            CategoryAllocationFlexibility.Flexible);

        var after = DateTimeOffset.UtcNow;
        var domainEvent = Assert.IsType<CategoryAllocationAddedEvent>(Assert.Single(budgetPlan.DomainEvents));
        Assert.Equal(budgetPlan.Id, domainEvent.BudgetPlanId);
        Assert.Equal(allocationId, domainEvent.CategoryAllocationId);
        Assert.Equal(category.Id, domainEvent.CategoryId);
        Assert.Equal(amount, domainEvent.Amount);
        Assert.Equal(CategoryAllocationFlexibility.Flexible, domainEvent.Flexibility);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void AddExpenseCategoryAllocation_Throws_WhenCategoryIsNull()
    {
        var budgetPlan = CreateBudgetPlan();

        Assert.Throws<ArgumentNullException>(() => budgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            null!,
            new Money(2500m, Currency.PLN),
            CategoryAllocationFlexibility.Fixed));
    }

    [Fact]
    public void AddExpenseCategoryAllocation_Throws_WhenCategoryIsIncomeCategory()
    {
        var budgetPlan = CreateBudgetPlan();
        var category = CreateCategory(budgetPlan.OwnerId, BudgetCategoryType.Income);

        Assert.Throws<ArgumentException>(() => budgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            category,
            new Money(2500m, Currency.PLN),
            CategoryAllocationFlexibility.Fixed));
    }

    [Fact]
    public void AddExpenseCategoryAllocation_Throws_WhenCategoryBelongsToDifferentOwner()
    {
        var budgetPlan = CreateBudgetPlan();
        var category = CreateExpenseCategory(new OwnerId(Guid.NewGuid()));

        Assert.Throws<InvalidOperationException>(() => budgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            category,
            new Money(2500m, Currency.PLN),
            CategoryAllocationFlexibility.Fixed));
    }

    [Fact]
    public void AddExpenseCategoryAllocation_Throws_WhenCategoryIsArchived()
    {
        var budgetPlan = CreateBudgetPlan();
        var category = CreateExpenseCategory(budgetPlan.OwnerId);
        category.Archive();

        Assert.Throws<InvalidOperationException>(() => budgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            category,
            new Money(2500m, Currency.PLN),
            CategoryAllocationFlexibility.Fixed));
    }

    [Fact]
    public void AddExpenseCategoryAllocation_Throws_WhenCategoryAlreadyHasAllocation()
    {
        var budgetPlan = CreateBudgetPlan();
        var category = CreateExpenseCategory(budgetPlan.OwnerId);
        budgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            category,
            new Money(2500m, Currency.PLN),
            CategoryAllocationFlexibility.Fixed);

        Assert.Throws<InvalidOperationException>(() => budgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            category,
            new Money(3000m, Currency.PLN),
            CategoryAllocationFlexibility.Flexible));
    }

    [Fact]
    public void AddPlannedIncome_Throws_WhenIdAlreadyExists()
    {
        var budgetPlan = CreateBudgetPlan();
        var plannedIncomeId = new PlannedIncomeId(Guid.NewGuid());

        budgetPlan.AddPlannedIncome(
            plannedIncomeId,
            CreateIncomeCategory(budgetPlan.OwnerId),
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10));

        Assert.Throws<InvalidOperationException>(() => budgetPlan.AddPlannedIncome(
            plannedIncomeId,
            CreateIncomeCategory(budgetPlan.OwnerId),
            "Bonus",
            new Money(1000m, Currency.PLN),
            new DateOnly(2026, 7, 20)));
    }

    [Fact]
    public void AddExpenseCategoryAllocation_Throws_WhenIdAlreadyExists()
    {
        var budgetPlan = CreateBudgetPlan();
        var allocationId = new CategoryAllocationId(Guid.NewGuid());

        budgetPlan.AddExpenseCategoryAllocation(
            allocationId,
            CreateExpenseCategory(budgetPlan.OwnerId),
            new Money(2500m, Currency.PLN),
            CategoryAllocationFlexibility.Fixed);

        Assert.Throws<InvalidOperationException>(() => budgetPlan.AddExpenseCategoryAllocation(
            allocationId,
            CreateExpenseCategory(budgetPlan.OwnerId),
            new Money(3000m, Currency.PLN),
            CategoryAllocationFlexibility.Flexible));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddExpenseCategoryAllocation_Throws_WhenAmountIsNotPositive(decimal amount)
    {
        var budgetPlan = CreateBudgetPlan();

        Assert.Throws<ArgumentOutOfRangeException>(() => budgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            CreateExpenseCategory(budgetPlan.OwnerId),
            new Money(amount, Currency.PLN),
            CategoryAllocationFlexibility.Fixed));
    }

    [Fact]
    public void AddExpenseCategoryAllocation_Throws_WhenAmountCurrencyDiffersFromDefaultCurrency()
    {
        var budgetPlan = CreateBudgetPlan();

        Assert.Throws<ArgumentException>(() => budgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            CreateExpenseCategory(budgetPlan.OwnerId),
            new Money(2500m, Currency.EUR),
            CategoryAllocationFlexibility.Fixed));
    }

    [Fact]
    public void AddExpenseCategoryAllocation_Throws_WhenFlexibilityIsInvalid()
    {
        var budgetPlan = CreateBudgetPlan();
        var invalidFlexibility = (CategoryAllocationFlexibility)999;

        Assert.Throws<ArgumentOutOfRangeException>(() => budgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            CreateExpenseCategory(budgetPlan.OwnerId),
            new Money(2500m, Currency.PLN),
            invalidFlexibility));
    }

    [Fact]
    public void AddSavingContribution_AddsSavingContribution()
    {
        var budgetPlan = CreateBudgetPlan();
        var contributionId = new SavingContributionId(Guid.NewGuid());
        var category = CreateSavingCategory(budgetPlan.OwnerId);
        var amount = new Money(1000m, Currency.PLN);

        var contribution = budgetPlan.AddSavingContribution(contributionId, category, amount);

        Assert.Equal(contributionId, contribution.Id);
        Assert.Equal(category.Id, contribution.CategoryId);
        Assert.Equal(amount, contribution.Amount);
        Assert.Contains(contribution, budgetPlan.SavingContributions);
        Assert.Equal(Money.Zero(Currency.PLN), budgetPlan.TotalPlannedIncome);
        Assert.Equal(Money.Zero(Currency.PLN), budgetPlan.TotalAllocatedExpenses);
        Assert.Equal(new Money(1000m, Currency.PLN), budgetPlan.TotalSavingContributions);
        Assert.Equal(new Money(-1000m, Currency.PLN), budgetPlan.PlannedFinancialResult);
        Assert.Equal(BudgetFitRisk.OptionalOverrun, budgetPlan.BudgetFitRisk);
    }

    [Fact]
    public void AddSavingContribution_RaisesSavingContributionAddedEvent()
    {
        var budgetPlan = CreateBudgetPlan();
        var contributionId = new SavingContributionId(Guid.NewGuid());
        var category = CreateSavingCategory(budgetPlan.OwnerId);
        var amount = new Money(1000m, Currency.PLN);
        var before = DateTimeOffset.UtcNow;

        budgetPlan.AddSavingContribution(contributionId, category, amount);

        var after = DateTimeOffset.UtcNow;
        var domainEvent = Assert.IsType<SavingContributionAddedEvent>(Assert.Single(budgetPlan.DomainEvents));
        Assert.Equal(budgetPlan.Id, domainEvent.BudgetPlanId);
        Assert.Equal(contributionId, domainEvent.SavingContributionId);
        Assert.Equal(category.Id, domainEvent.CategoryId);
        Assert.Equal(amount, domainEvent.Amount);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void AddSavingContribution_Throws_WhenCategoryIsNull()
    {
        var budgetPlan = CreateBudgetPlan();

        Assert.Throws<ArgumentNullException>(() => budgetPlan.AddSavingContribution(
            new SavingContributionId(Guid.NewGuid()),
            null!,
            new Money(1000m, Currency.PLN)));
    }

    [Theory]
    [InlineData(BudgetCategoryType.Income)]
    [InlineData(BudgetCategoryType.Expense)]
    public void AddSavingContribution_Throws_WhenCategoryIsNotSavingCategory(BudgetCategoryType categoryType)
    {
        var budgetPlan = CreateBudgetPlan();
        var category = CreateCategory(budgetPlan.OwnerId, categoryType);

        Assert.Throws<ArgumentException>(() => budgetPlan.AddSavingContribution(
            new SavingContributionId(Guid.NewGuid()),
            category,
            new Money(1000m, Currency.PLN)));
    }

    [Fact]
    public void AddSavingContribution_Throws_WhenCategoryBelongsToDifferentOwner()
    {
        var budgetPlan = CreateBudgetPlan();
        var category = CreateSavingCategory(new OwnerId(Guid.NewGuid()));

        Assert.Throws<InvalidOperationException>(() => budgetPlan.AddSavingContribution(
            new SavingContributionId(Guid.NewGuid()),
            category,
            new Money(1000m, Currency.PLN)));
    }

    [Fact]
    public void AddSavingContribution_Throws_WhenCategoryIsArchived()
    {
        var budgetPlan = CreateBudgetPlan();
        var category = CreateSavingCategory(budgetPlan.OwnerId);
        category.Archive();

        Assert.Throws<InvalidOperationException>(() => budgetPlan.AddSavingContribution(
            new SavingContributionId(Guid.NewGuid()),
            category,
            new Money(1000m, Currency.PLN)));
    }

    [Fact]
    public void AddSavingContribution_Throws_WhenCategoryAlreadyHasContribution()
    {
        var budgetPlan = CreateBudgetPlan();
        var category = CreateSavingCategory(budgetPlan.OwnerId);
        budgetPlan.AddSavingContribution(
            new SavingContributionId(Guid.NewGuid()),
            category,
            new Money(1000m, Currency.PLN));

        Assert.Throws<InvalidOperationException>(() => budgetPlan.AddSavingContribution(
            new SavingContributionId(Guid.NewGuid()),
            category,
            new Money(1500m, Currency.PLN)));
    }

    [Fact]
    public void AddSavingContribution_Throws_WhenIdAlreadyExists()
    {
        var budgetPlan = CreateBudgetPlan();
        var contributionId = new SavingContributionId(Guid.NewGuid());

        budgetPlan.AddSavingContribution(
            contributionId,
            CreateSavingCategory(budgetPlan.OwnerId),
            new Money(1000m, Currency.PLN));

        Assert.Throws<InvalidOperationException>(() => budgetPlan.AddSavingContribution(
            contributionId,
            CreateSavingCategory(budgetPlan.OwnerId),
            new Money(1500m, Currency.PLN)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddSavingContribution_Throws_WhenAmountIsNotPositive(decimal amount)
    {
        var budgetPlan = CreateBudgetPlan();

        Assert.Throws<ArgumentOutOfRangeException>(() => budgetPlan.AddSavingContribution(
            new SavingContributionId(Guid.NewGuid()),
            CreateSavingCategory(budgetPlan.OwnerId),
            new Money(amount, Currency.PLN)));
    }

    [Fact]
    public void AddSavingContribution_Throws_WhenAmountCurrencyDiffersFromDefaultCurrency()
    {
        var budgetPlan = CreateBudgetPlan();

        Assert.Throws<ArgumentException>(() => budgetPlan.AddSavingContribution(
            new SavingContributionId(Guid.NewGuid()),
            CreateSavingCategory(budgetPlan.OwnerId),
            new Money(1000m, Currency.EUR)));
    }

    [Fact]
    public void CategoryAllocationPercentages_AreRecalculatedFromExpensesAndIncome()
    {
        var budgetPlan = CreateBudgetPlan();
        AddIncome(budgetPlan, 10000m);

        var fixedAllocation = AddAllocation(budgetPlan, 2500m, CategoryAllocationFlexibility.Fixed);
        var optionalAllocation = AddAllocation(budgetPlan, 1500m, CategoryAllocationFlexibility.Optional);

        Assert.Equal(62.5m, fixedAllocation.ExpenseSharePercentage);
        Assert.Equal(25m, fixedAllocation.IncomeSharePercentage);
        Assert.Equal(37.5m, optionalAllocation.ExpenseSharePercentage);
        Assert.Equal(15m, optionalAllocation.IncomeSharePercentage);
        Assert.Equal(BudgetFitRisk.Balanced, budgetPlan.BudgetFitRisk);
        Assert.Equal(new Money(10000m, Currency.PLN), budgetPlan.TotalPlannedIncome);
        Assert.Equal(new Money(4000m, Currency.PLN), budgetPlan.TotalAllocatedExpenses);
        Assert.Equal(Money.Zero(Currency.PLN), budgetPlan.TotalSavingContributions);
        Assert.Equal(new Money(6000m, Currency.PLN), budgetPlan.PlannedFinancialResult);
    }

    [Fact]
    public void CategoryAllocationPercentages_UseConvertedIncomeAmount()
    {
        var budgetPlan = CreateBudgetPlan();
        budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budgetPlan.OwnerId),
            "Salary",
            new Money(1000m, Currency.EUR),
            new DateOnly(2026, 7, 10),
            new Money(4250m, Currency.PLN),
            new DateOnly(2026, 7, 10));

        var allocation = AddAllocation(budgetPlan, 425m, CategoryAllocationFlexibility.Fixed);

        Assert.Equal(10m, allocation.IncomeSharePercentage);
        Assert.Equal(BudgetFitRisk.Balanced, budgetPlan.BudgetFitRisk);
        Assert.Equal(new Money(4250m, Currency.PLN), budgetPlan.TotalPlannedIncome);
        Assert.Equal(new Money(425m, Currency.PLN), budgetPlan.TotalAllocatedExpenses);
        Assert.Equal(Money.Zero(Currency.PLN), budgetPlan.TotalSavingContributions);
        Assert.Equal(new Money(3825m, Currency.PLN), budgetPlan.PlannedFinancialResult);
    }

    [Fact]
    public void ChangeExpenseCategoryAllocationAmount_ChangesAmountAndRecalculatesPercentages()
    {
        var budgetPlan = CreateBudgetPlan();
        AddIncome(budgetPlan, 1000m);
        var fixedAllocation = AddAllocation(budgetPlan, 250m, CategoryAllocationFlexibility.Fixed);
        var optionalAllocation = AddAllocation(budgetPlan, 250m, CategoryAllocationFlexibility.Optional);

        budgetPlan.ChangeExpenseCategoryAllocationAmount(fixedAllocation.Id, new Money(750m, Currency.PLN));

        Assert.Equal(new Money(750m, Currency.PLN), fixedAllocation.Amount);
        Assert.Equal(75m, fixedAllocation.ExpenseSharePercentage);
        Assert.Equal(75m, fixedAllocation.IncomeSharePercentage);
        Assert.Equal(25m, optionalAllocation.ExpenseSharePercentage);
        Assert.Equal(25m, optionalAllocation.IncomeSharePercentage);
        Assert.Equal(new Money(1000m, Currency.PLN), budgetPlan.TotalPlannedIncome);
        Assert.Equal(new Money(1000m, Currency.PLN), budgetPlan.TotalAllocatedExpenses);
        Assert.Equal(Money.Zero(Currency.PLN), budgetPlan.TotalSavingContributions);
        Assert.Equal(Money.Zero(Currency.PLN), budgetPlan.PlannedFinancialResult);
    }

    [Fact]
    public void ChangeExpenseCategoryAllocationAmount_RaisesCategoryAllocationAmountChangedEvent()
    {
        var budgetPlan = CreateBudgetPlan();
        var allocation = AddAllocation(budgetPlan, 250m, CategoryAllocationFlexibility.Fixed);
        var newAmount = new Money(750m, Currency.PLN);
        budgetPlan.ClearDomainEvents();
        var before = DateTimeOffset.UtcNow;

        budgetPlan.ChangeExpenseCategoryAllocationAmount(allocation.Id, newAmount);

        var after = DateTimeOffset.UtcNow;
        var domainEvent = Assert.IsType<CategoryAllocationAmountChangedEvent>(Assert.Single(budgetPlan.DomainEvents));
        Assert.Equal(budgetPlan.Id, domainEvent.BudgetPlanId);
        Assert.Equal(allocation.Id, domainEvent.CategoryAllocationId);
        Assert.Equal(allocation.CategoryId, domainEvent.CategoryId);
        Assert.Equal(new Money(250m, Currency.PLN), domainEvent.PreviousAmount);
        Assert.Equal(newAmount, domainEvent.NewAmount);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void ChangeExpenseCategoryAllocationAmount_Throws_WhenAmountCurrencyDiffersFromDefaultCurrency()
    {
        var budgetPlan = CreateBudgetPlan();
        var allocation = AddAllocation(budgetPlan, 250m, CategoryAllocationFlexibility.Fixed);

        Assert.Throws<ArgumentException>(() => budgetPlan.ChangeExpenseCategoryAllocationAmount(
            allocation.Id,
            new Money(250m, Currency.EUR)));
    }

    [Fact]
    public void ChangeExpenseCategoryAllocationFlexibility_ChangesFlexibilityAndRecalculatesRisk()
    {
        var budgetPlan = CreateBudgetPlan();
        AddIncome(budgetPlan, 1000m);
        AddAllocation(budgetPlan, 700m, CategoryAllocationFlexibility.Fixed);
        var allocation = AddAllocation(budgetPlan, 400m, CategoryAllocationFlexibility.Optional);
        Assert.Equal(BudgetFitRisk.OptionalOverrun, budgetPlan.BudgetFitRisk);

        budgetPlan.ChangeExpenseCategoryAllocationFlexibility(allocation.Id, CategoryAllocationFlexibility.Flexible);

        Assert.Equal(CategoryAllocationFlexibility.Flexible, allocation.Flexibility);
        Assert.Equal(BudgetFitRisk.FlexibleOverrun, budgetPlan.BudgetFitRisk);
    }

    [Fact]
    public void ChangeExpenseCategoryAllocationFlexibility_RaisesCategoryAllocationFlexibilityChangedEvent()
    {
        var budgetPlan = CreateBudgetPlan();
        var allocation = AddAllocation(budgetPlan, 250m, CategoryAllocationFlexibility.Optional);
        budgetPlan.ClearDomainEvents();
        var before = DateTimeOffset.UtcNow;

        budgetPlan.ChangeExpenseCategoryAllocationFlexibility(allocation.Id, CategoryAllocationFlexibility.Flexible);

        var after = DateTimeOffset.UtcNow;
        var domainEvent = Assert.IsType<CategoryAllocationFlexibilityChangedEvent>(Assert.Single(budgetPlan.DomainEvents));
        Assert.Equal(budgetPlan.Id, domainEvent.BudgetPlanId);
        Assert.Equal(allocation.Id, domainEvent.CategoryAllocationId);
        Assert.Equal(allocation.CategoryId, domainEvent.CategoryId);
        Assert.Equal(CategoryAllocationFlexibility.Optional, domainEvent.PreviousFlexibility);
        Assert.Equal(CategoryAllocationFlexibility.Flexible, domainEvent.NewFlexibility);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void ChangeExpenseCategoryAllocationFlexibility_Throws_WhenFlexibilityIsInvalid()
    {
        var budgetPlan = CreateBudgetPlan();
        var allocation = AddAllocation(budgetPlan, 250m, CategoryAllocationFlexibility.Fixed);
        var invalidFlexibility = (CategoryAllocationFlexibility)999;

        Assert.Throws<ArgumentOutOfRangeException>(() => budgetPlan.ChangeExpenseCategoryAllocationFlexibility(
            allocation.Id,
            invalidFlexibility));
    }

    [Fact]
    public void RemoveExpenseCategoryAllocation_RemovesAllocationAndRecalculatesRisk()
    {
        var budgetPlan = CreateBudgetPlan();
        AddIncome(budgetPlan, 1000m);
        AddAllocation(budgetPlan, 500m, CategoryAllocationFlexibility.Fixed);
        var allocation = AddAllocation(budgetPlan, 600m, CategoryAllocationFlexibility.Optional);
        Assert.Equal(BudgetFitRisk.OptionalOverrun, budgetPlan.BudgetFitRisk);

        budgetPlan.RemoveExpenseCategoryAllocation(allocation.Id);

        Assert.DoesNotContain(allocation, budgetPlan.ExpenseCategoryAllocations);
        Assert.Equal(new Money(500m, Currency.PLN), budgetPlan.TotalAllocatedExpenses);
        Assert.Equal(new Money(500m, Currency.PLN), budgetPlan.PlannedFinancialResult);
        Assert.Equal(BudgetFitRisk.Balanced, budgetPlan.BudgetFitRisk);
    }

    [Fact]
    public void RemoveExpenseCategoryAllocation_RaisesCategoryAllocationRemovedEvent()
    {
        var budgetPlan = CreateBudgetPlan();
        var allocation = AddAllocation(budgetPlan, 600m, CategoryAllocationFlexibility.Optional);
        budgetPlan.ClearDomainEvents();
        var before = DateTimeOffset.UtcNow;

        budgetPlan.RemoveExpenseCategoryAllocation(allocation.Id);

        var after = DateTimeOffset.UtcNow;
        var domainEvent = Assert.IsType<CategoryAllocationRemovedEvent>(Assert.Single(budgetPlan.DomainEvents));
        Assert.Equal(budgetPlan.Id, domainEvent.BudgetPlanId);
        Assert.Equal(allocation.Id, domainEvent.CategoryAllocationId);
        Assert.Equal(allocation.CategoryId, domainEvent.CategoryId);
        Assert.Equal(allocation.Amount, domainEvent.Amount);
        Assert.Equal(allocation.Flexibility, domainEvent.Flexibility);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void RemoveExpenseCategoryAllocation_Throws_WhenAllocationIsFixed()
    {
        var budgetPlan = CreateBudgetPlan();
        var allocation = AddAllocation(budgetPlan, 500m, CategoryAllocationFlexibility.Fixed);

        Assert.Throws<InvalidOperationException>(() => budgetPlan.RemoveExpenseCategoryAllocation(allocation.Id));
        Assert.Contains(allocation, budgetPlan.ExpenseCategoryAllocations);
    }

    [Fact]
    public void ChangeSavingContributionAmount_ChangesAmountAndRecalculatesRisk()
    {
        var budgetPlan = CreateBudgetPlan();
        AddIncome(budgetPlan, 5000m);
        AddAllocation(budgetPlan, 3500m, CategoryAllocationFlexibility.Fixed);
        var contribution = AddSavingContribution(budgetPlan, 1000m);
        Assert.Equal(BudgetFitRisk.Balanced, budgetPlan.BudgetFitRisk);

        budgetPlan.ChangeSavingContributionAmount(contribution.Id, new Money(2000m, Currency.PLN));

        Assert.Equal(new Money(2000m, Currency.PLN), contribution.Amount);
        Assert.Equal(new Money(2000m, Currency.PLN), budgetPlan.TotalSavingContributions);
        Assert.Equal(new Money(-500m, Currency.PLN), budgetPlan.PlannedFinancialResult);
        Assert.Equal(BudgetFitRisk.OptionalOverrun, budgetPlan.BudgetFitRisk);
    }

    [Fact]
    public void ChangeSavingContributionAmount_RaisesSavingContributionAmountChangedEvent()
    {
        var budgetPlan = CreateBudgetPlan();
        var contribution = AddSavingContribution(budgetPlan, 1000m);
        var newAmount = new Money(2000m, Currency.PLN);
        budgetPlan.ClearDomainEvents();
        var before = DateTimeOffset.UtcNow;

        budgetPlan.ChangeSavingContributionAmount(contribution.Id, newAmount);

        var after = DateTimeOffset.UtcNow;
        var domainEvent = Assert.IsType<SavingContributionAmountChangedEvent>(Assert.Single(budgetPlan.DomainEvents));
        Assert.Equal(budgetPlan.Id, domainEvent.BudgetPlanId);
        Assert.Equal(contribution.Id, domainEvent.SavingContributionId);
        Assert.Equal(contribution.CategoryId, domainEvent.CategoryId);
        Assert.Equal(new Money(1000m, Currency.PLN), domainEvent.PreviousAmount);
        Assert.Equal(newAmount, domainEvent.NewAmount);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void ChangeSavingContributionAmount_Throws_WhenAmountCurrencyDiffersFromDefaultCurrency()
    {
        var budgetPlan = CreateBudgetPlan();
        var contribution = AddSavingContribution(budgetPlan, 1000m);

        Assert.Throws<ArgumentException>(() => budgetPlan.ChangeSavingContributionAmount(
            contribution.Id,
            new Money(1000m, Currency.EUR)));
    }

    [Fact]
    public void RemoveSavingContribution_RemovesContributionAndRecalculatesRisk()
    {
        var budgetPlan = CreateBudgetPlan();
        AddIncome(budgetPlan, 5000m);
        AddAllocation(budgetPlan, 4000m, CategoryAllocationFlexibility.Fixed);
        var contribution = AddSavingContribution(budgetPlan, 1500m);
        Assert.Equal(BudgetFitRisk.OptionalOverrun, budgetPlan.BudgetFitRisk);

        budgetPlan.RemoveSavingContribution(contribution.Id);

        Assert.DoesNotContain(contribution, budgetPlan.SavingContributions);
        Assert.Equal(Money.Zero(Currency.PLN), budgetPlan.TotalSavingContributions);
        Assert.Equal(new Money(1000m, Currency.PLN), budgetPlan.PlannedFinancialResult);
        Assert.Equal(BudgetFitRisk.Balanced, budgetPlan.BudgetFitRisk);
    }

    [Fact]
    public void RemoveSavingContribution_RaisesSavingContributionRemovedEvent()
    {
        var budgetPlan = CreateBudgetPlan();
        var contribution = AddSavingContribution(budgetPlan, 1500m);
        budgetPlan.ClearDomainEvents();
        var before = DateTimeOffset.UtcNow;

        budgetPlan.RemoveSavingContribution(contribution.Id);

        var after = DateTimeOffset.UtcNow;
        var domainEvent = Assert.IsType<SavingContributionRemovedEvent>(Assert.Single(budgetPlan.DomainEvents));
        Assert.Equal(budgetPlan.Id, domainEvent.BudgetPlanId);
        Assert.Equal(contribution.Id, domainEvent.SavingContributionId);
        Assert.Equal(contribution.CategoryId, domainEvent.CategoryId);
        Assert.Equal(contribution.Amount, domainEvent.Amount);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void BudgetFitRisk_IsBalanced_WhenAllAllocationsFitIncome()
    {
        var budgetPlan = CreateBudgetPlan();
        AddIncome(budgetPlan, 1000m);

        AddAllocation(budgetPlan, 400m, CategoryAllocationFlexibility.Fixed);
        AddAllocation(budgetPlan, 600m, CategoryAllocationFlexibility.Optional);

        Assert.Equal(BudgetFitRisk.Balanced, budgetPlan.BudgetFitRisk);
    }

    [Fact]
    public void BudgetFitRisk_IsBalanced_WhenExpensesAndSavingContributionsFitIncome()
    {
        var budgetPlan = CreateBudgetPlan();
        AddIncome(budgetPlan, 5000m);

        AddAllocation(budgetPlan, 4000m, CategoryAllocationFlexibility.Fixed);
        AddSavingContribution(budgetPlan, 1000m);

        Assert.Equal(BudgetFitRisk.Balanced, budgetPlan.BudgetFitRisk);
        Assert.Equal(Money.Zero(Currency.PLN), budgetPlan.PlannedFinancialResult);
    }

    [Fact]
    public void BudgetFitRisk_IsOptionalOverrun_WhenOnlyOptionalAllocationsNeedToBeReduced()
    {
        var budgetPlan = CreateBudgetPlan();
        AddIncome(budgetPlan, 1000m);

        AddAllocation(budgetPlan, 500m, CategoryAllocationFlexibility.Fixed);
        AddAllocation(budgetPlan, 400m, CategoryAllocationFlexibility.Flexible);
        AddAllocation(budgetPlan, 200m, CategoryAllocationFlexibility.Optional);

        Assert.Equal(BudgetFitRisk.OptionalOverrun, budgetPlan.BudgetFitRisk);
    }

    [Fact]
    public void BudgetFitRisk_IsOptionalOverrun_WhenSavingContributionsExceedRemainingIncome()
    {
        var budgetPlan = CreateBudgetPlan();
        AddIncome(budgetPlan, 5000m);

        AddAllocation(budgetPlan, 4000m, CategoryAllocationFlexibility.Fixed);
        AddSavingContribution(budgetPlan, 1500m);

        Assert.Equal(BudgetFitRisk.OptionalOverrun, budgetPlan.BudgetFitRisk);
        Assert.Equal(new Money(-500m, Currency.PLN), budgetPlan.PlannedFinancialResult);
    }

    [Fact]
    public void BudgetFitRisk_IsFlexibleOverrun_WhenFlexibleAllocationsNeedToBeReduced()
    {
        var budgetPlan = CreateBudgetPlan();
        AddIncome(budgetPlan, 1000m);

        AddAllocation(budgetPlan, 500m, CategoryAllocationFlexibility.Fixed);
        AddAllocation(budgetPlan, 600m, CategoryAllocationFlexibility.Flexible);
        AddAllocation(budgetPlan, 100m, CategoryAllocationFlexibility.Optional);

        Assert.Equal(BudgetFitRisk.FlexibleOverrun, budgetPlan.BudgetFitRisk);
    }

    [Fact]
    public void BudgetFitRisk_IsFlexibleOverrun_WhenFixedAndFlexibleExpensesExceedIncomeWithSavingContributions()
    {
        var budgetPlan = CreateBudgetPlan();
        AddIncome(budgetPlan, 5000m);

        AddAllocation(budgetPlan, 3000m, CategoryAllocationFlexibility.Fixed);
        AddAllocation(budgetPlan, 2500m, CategoryAllocationFlexibility.Flexible);
        AddSavingContribution(budgetPlan, 100m);

        Assert.Equal(BudgetFitRisk.FlexibleOverrun, budgetPlan.BudgetFitRisk);
    }

    [Fact]
    public void BudgetFitRisk_IsFixedOverrun_WhenFixedAllocationsExceedIncome()
    {
        var budgetPlan = CreateBudgetPlan();
        AddIncome(budgetPlan, 1000m);

        AddAllocation(budgetPlan, 1001m, CategoryAllocationFlexibility.Fixed);

        Assert.Equal(BudgetFitRisk.FixedOverrun, budgetPlan.BudgetFitRisk);
    }

    [Fact]
    public void BudgetFitRisk_IsFixedOverrun_WhenFixedExpensesExceedIncomeWithSavingContributions()
    {
        var budgetPlan = CreateBudgetPlan();
        AddIncome(budgetPlan, 5000m);

        AddAllocation(budgetPlan, 5500m, CategoryAllocationFlexibility.Fixed);
        AddSavingContribution(budgetPlan, 100m);

        Assert.Equal(BudgetFitRisk.FixedOverrun, budgetPlan.BudgetFitRisk);
    }

    [Fact]
    public void Activate_ChangesStatusAndRaisesStatusChangedEvent()
    {
        var budgetPlan = CreateBudgetPlan();
        var before = DateTimeOffset.UtcNow;

        budgetPlan.Activate();

        var after = DateTimeOffset.UtcNow;
        Assert.Equal(BudgetPlanStatus.Active, budgetPlan.Status);
        var domainEvent = Assert.IsType<BudgetPlanStatusChangedEvent>(Assert.Single(budgetPlan.DomainEvents));
        Assert.Equal(budgetPlan.Id, domainEvent.BudgetPlanId);
        Assert.Equal(BudgetPlanStatus.Draft, domainEvent.PreviousStatus);
        Assert.Equal(BudgetPlanStatus.Active, domainEvent.NewStatus);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void Activate_Throws_WhenPlanIsNotDraft()
    {
        var budgetPlan = CreateBudgetPlan();
        budgetPlan.Activate();

        Assert.Throws<InvalidOperationException>(budgetPlan.Activate);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Close_ChangesStatusAndRaisesStatusChangedEvent(bool activateBeforeClose)
    {
        var budgetPlan = CreateBudgetPlan();

        if (activateBeforeClose)
        {
            budgetPlan.Activate();
            budgetPlan.ClearDomainEvents();
        }

        var expectedPreviousStatus = budgetPlan.Status;
        var before = DateTimeOffset.UtcNow;

        budgetPlan.Close();

        var after = DateTimeOffset.UtcNow;
        Assert.Equal(BudgetPlanStatus.Closed, budgetPlan.Status);
        var domainEvent = Assert.IsType<BudgetPlanStatusChangedEvent>(Assert.Single(budgetPlan.DomainEvents));
        Assert.Equal(budgetPlan.Id, domainEvent.BudgetPlanId);
        Assert.Equal(expectedPreviousStatus, domainEvent.PreviousStatus);
        Assert.Equal(BudgetPlanStatus.Closed, domainEvent.NewStatus);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void Close_Throws_WhenPlanIsAlreadyClosed()
    {
        var budgetPlan = CreateBudgetPlan();
        budgetPlan.Close();

        Assert.Throws<InvalidOperationException>(budgetPlan.Close);
    }

    [Fact]
    public void CopyTo_CopiesPlanItemsToTargetPeriod()
    {
        var budgetPlan = new BudgetPlan(
            new BudgetPlanId(Guid.NewGuid()),
            new OwnerId(Guid.NewGuid()),
            new BudgetPeriod(2026, 1),
            Currency.PLN);
        var income = budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budgetPlan.OwnerId),
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 1, 31));
        var allocation = budgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            CreateExpenseCategory(budgetPlan.OwnerId),
            new Money(3000m, Currency.PLN),
            CategoryAllocationFlexibility.Fixed);
        var contribution = budgetPlan.AddSavingContribution(
            new SavingContributionId(Guid.NewGuid()),
            CreateSavingCategory(budgetPlan.OwnerId),
            new Money(500m, Currency.PLN));
        budgetPlan.Activate();

        var copy = budgetPlan.CopyTo(
            new BudgetPlanId(Guid.NewGuid()),
            new BudgetPeriod(2026, 2),
            () => new PlannedIncomeId(Guid.NewGuid()),
            () => new CategoryAllocationId(Guid.NewGuid()),
            () => new SavingContributionId(Guid.NewGuid()));

        Assert.NotEqual(budgetPlan.Id, copy.Id);
        Assert.Equal(budgetPlan.OwnerId, copy.OwnerId);
        Assert.Equal(new BudgetPeriod(2026, 2), copy.Period);
        Assert.Equal(budgetPlan.DefaultCurrency, copy.DefaultCurrency);
        Assert.Equal(BudgetPlanStatus.Draft, copy.Status);
        Assert.Equal(new Money(5000m, Currency.PLN), copy.TotalPlannedIncome);
        Assert.Equal(new Money(3000m, Currency.PLN), copy.TotalAllocatedExpenses);
        Assert.Equal(new Money(500m, Currency.PLN), copy.TotalSavingContributions);
        Assert.Equal(new Money(1500m, Currency.PLN), copy.PlannedFinancialResult);
        Assert.Equal(BudgetFitRisk.Balanced, copy.BudgetFitRisk);

        var copiedIncome = Assert.Single(copy.PlannedIncomes);
        Assert.NotEqual(income.Id, copiedIncome.Id);
        Assert.Equal(income.CategoryId, copiedIncome.CategoryId);
        Assert.Equal(income.Title, copiedIncome.Title);
        Assert.Equal(income.Amount, copiedIncome.Amount);
        Assert.Equal(new DateOnly(2026, 2, 28), copiedIncome.ExpectedDate);

        var copiedAllocation = Assert.Single(copy.ExpenseCategoryAllocations);
        Assert.NotEqual(allocation.Id, copiedAllocation.Id);
        Assert.Equal(allocation.CategoryId, copiedAllocation.CategoryId);
        Assert.Equal(allocation.Amount, copiedAllocation.Amount);
        Assert.Equal(allocation.Flexibility, copiedAllocation.Flexibility);

        var copiedContribution = Assert.Single(copy.SavingContributions);
        Assert.NotEqual(contribution.Id, copiedContribution.Id);
        Assert.Equal(contribution.CategoryId, copiedContribution.CategoryId);
        Assert.Equal(contribution.Amount, copiedContribution.Amount);
    }

    [Fact]
    public void CopyTo_SkipsDisabledSections()
    {
        var budgetPlan = CreateBudgetPlan();
        AddIncome(budgetPlan, 5000m);
        AddAllocation(budgetPlan, 3000m, CategoryAllocationFlexibility.Fixed);
        AddSavingContribution(budgetPlan, 500m);

        var copy = budgetPlan.CopyTo(
            new BudgetPlanId(Guid.NewGuid()),
            new BudgetPeriod(2026, 8),
            () => new PlannedIncomeId(Guid.NewGuid()),
            () => new CategoryAllocationId(Guid.NewGuid()),
            () => new SavingContributionId(Guid.NewGuid()),
            copyPlannedIncomes: false,
            copyExpenseCategoryAllocations: false,
            copySavingContributions: false);

        Assert.Empty(copy.PlannedIncomes);
        Assert.Empty(copy.ExpenseCategoryAllocations);
        Assert.Empty(copy.SavingContributions);
        Assert.Equal(Money.Zero(Currency.PLN), copy.TotalPlannedIncome);
        Assert.Equal(Money.Zero(Currency.PLN), copy.TotalAllocatedExpenses);
        Assert.Equal(Money.Zero(Currency.PLN), copy.TotalSavingContributions);
        Assert.Equal(Money.Zero(Currency.PLN), copy.PlannedFinancialResult);
    }

    [Fact]
    public void AddPlannedIncome_Throws_WhenPlanIsNotDraft()
    {
        var budgetPlan = CreateBudgetPlan();
        budgetPlan.Activate();

        Assert.Throws<InvalidOperationException>(() => budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budgetPlan.OwnerId),
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10)));
    }

    private static BudgetPlan CreateBudgetPlan()
        => new(
            new BudgetPlanId(Guid.NewGuid()),
            new OwnerId(Guid.NewGuid()),
            new BudgetPeriod(2026, 7),
            Currency.PLN);

    private static void AddIncome(BudgetPlan budgetPlan, decimal amount)
    {
        budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budgetPlan.OwnerId),
            "Salary",
            new Money(amount, Currency.PLN),
            new DateOnly(2026, 7, 10));
    }

    private static CategoryAllocation AddAllocation(
        BudgetPlan budgetPlan,
        decimal amount,
        CategoryAllocationFlexibility flexibility)
        => budgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            CreateExpenseCategory(budgetPlan.OwnerId),
            new Money(amount, Currency.PLN),
            flexibility);

    private static SavingContribution AddSavingContribution(BudgetPlan budgetPlan, decimal amount)
        => budgetPlan.AddSavingContribution(
            new SavingContributionId(Guid.NewGuid()),
            CreateSavingCategory(budgetPlan.OwnerId),
            new Money(amount, Currency.PLN));

    private static BudgetCategory CreateExpenseCategory(OwnerId ownerId)
        => CreateCategory(ownerId, BudgetCategoryType.Expense);

    private static BudgetCategory CreateIncomeCategory(OwnerId ownerId)
        => CreateCategory(ownerId, BudgetCategoryType.Income);

    private static BudgetCategory CreateSavingCategory(OwnerId ownerId)
        => CreateCategory(ownerId, BudgetCategoryType.Saving);

    private static BudgetCategory CreateCategory(OwnerId ownerId, BudgetCategoryType type)
        => new(
            new BudgetCategoryId(Guid.NewGuid()),
            ownerId,
            "Category",
            type);
}
