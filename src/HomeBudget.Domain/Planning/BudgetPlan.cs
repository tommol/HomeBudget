using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Planning;

/// <summary>
/// Represents a budget plan entity.
/// </summary>
public sealed class BudgetPlan : AggregateRoot<BudgetPlanId>
{
    private readonly List<PlannedIncome> _plannedIncomes = [];
    private readonly List<CategoryAllocation> _expenseCategoryAllocations = [];
    private readonly List<SavingContribution> _savingContributions = [];

    public BudgetPlan(
        BudgetPlanId id,
        OwnerId ownerId,
        BudgetPeriod period,
        Currency defaultCurrency)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(ownerId);
        ArgumentNullException.ThrowIfNull(period);
        ArgumentNullException.ThrowIfNull(defaultCurrency);

        OwnerId = ownerId;
        Period = period;
        DefaultCurrency = defaultCurrency;
        Status = BudgetPlanStatus.Draft;
        BudgetFitRisk = BudgetFitRisk.Balanced;
        TotalPlannedIncome = Money.Zero(defaultCurrency);
        TotalAllocatedExpenses = Money.Zero(defaultCurrency);
        TotalSavingContributions = Money.Zero(defaultCurrency);
        PlannedFinancialResult = Money.Zero(defaultCurrency);
    }

    public OwnerId OwnerId { get; private set; }
    public BudgetPlanStatus Status { get; private set; }
    public BudgetFitRisk BudgetFitRisk { get; private set; }
    public Money TotalPlannedIncome { get; private set; }
    public Money TotalAllocatedExpenses { get; private set; }
    public Money TotalSavingContributions { get; private set; }
    public Money PlannedFinancialResult { get; private set; }
    public BudgetPeriod Period { get; private set; }
    public Currency DefaultCurrency { get; private set; }
    public IReadOnlyCollection<PlannedIncome> PlannedIncomes => _plannedIncomes.AsReadOnly();
    public IReadOnlyCollection<CategoryAllocation> ExpenseCategoryAllocations => _expenseCategoryAllocations.AsReadOnly();
    public IReadOnlyCollection<SavingContribution> SavingContributions => _savingContributions.AsReadOnly();

    public PlannedIncome AddPlannedIncome(
        PlannedIncomeId id,
        BudgetCategory category,
        string title,
        Money amount,
        DateOnly expectedDate,
        Money? convertedAmount = null,
        DateOnly? conversionDate = null)
    {
        EnsureCanBeModified();
        EnsurePlannedIncomeIdIsUnique(id);
        EnsureCanUseIncomeCategory(category);
        EnsureDateIsInsidePeriod(expectedDate);
        EnsureIncomeAmountCanBeUsedForPlan(amount, convertedAmount, conversionDate);

        var plannedIncome = new PlannedIncome(id, category.Id, title, amount, expectedDate, convertedAmount, conversionDate);

        _plannedIncomes.Add(plannedIncome);
        RecalculateAllocations();
        RaiseDomainEvent(new PlannedIncomeAddedEvent(
            Id,
            plannedIncome.Id,
            plannedIncome.CategoryId,
            plannedIncome.Title,
            plannedIncome.Amount,
            plannedIncome.ConvertedAmount,
            plannedIncome.ConversionDate,
            plannedIncome.ExpectedDate,
            DateTimeOffset.UtcNow));

        return plannedIncome;
    }

    public CategoryAllocation AddExpenseCategoryAllocation(
        CategoryAllocationId id,
        BudgetCategory category,
        Money amount,
        CategoryAllocationFlexibility flexibility)
    {
        EnsureCanBeModified();
        EnsureCategoryAllocationIdIsUnique(id);
        EnsureCanAllocateExpenseCategory(category);
        EnsureAllocationAmountUsesDefaultCurrency(amount);
        EnsureCategoryHasNoAllocation(category.Id);

        var allocation = new CategoryAllocation(id, category.Id, amount, flexibility);

        _expenseCategoryAllocations.Add(allocation);
        RecalculateAllocations();
        RaiseDomainEvent(new CategoryAllocationAddedEvent(
            Id,
            allocation.Id,
            allocation.CategoryId,
            allocation.Amount,
            allocation.Flexibility,
            DateTimeOffset.UtcNow));

        return allocation;
    }

    public void ChangeExpenseCategoryAllocationAmount(CategoryAllocationId id, Money amount)
    {
        EnsureCanBeModified();
        EnsureAllocationAmountUsesDefaultCurrency(amount);

        var allocation = GetExpenseCategoryAllocation(id);

        allocation.ChangeAmount(amount);
        RecalculateAllocations();
    }

    public void ChangeExpenseCategoryAllocationFlexibility(CategoryAllocationId id, CategoryAllocationFlexibility flexibility)
    {
        EnsureCanBeModified();

        var allocation = GetExpenseCategoryAllocation(id);

        allocation.ChangeFlexibility(flexibility);
        RecalculateAllocations();
    }

    public SavingContribution AddSavingContribution(
        SavingContributionId id,
        BudgetCategory category,
        Money amount)
    {
        EnsureCanBeModified();
        EnsureSavingContributionIdIsUnique(id);
        EnsureCanUseSavingCategory(category);
        EnsureSavingContributionAmountUsesDefaultCurrency(amount);
        EnsureCategoryHasNoSavingContribution(category.Id);

        var contribution = new SavingContribution(id, category.Id, amount);

        _savingContributions.Add(contribution);
        RecalculateAllocations();
        RaiseDomainEvent(new SavingContributionAddedEvent(
            Id,
            contribution.Id,
            contribution.CategoryId,
            contribution.Amount,
            DateTimeOffset.UtcNow));

        return contribution;
    }

    public void ChangeSavingContributionAmount(SavingContributionId id, Money amount)
    {
        EnsureCanBeModified();
        EnsureSavingContributionAmountUsesDefaultCurrency(amount);

        var contribution = GetSavingContribution(id);

        contribution.ChangeAmount(amount);
        RecalculateAllocations();
    }

    public void Activate()
    {
        if (Status != BudgetPlanStatus.Draft)
        {
            throw new InvalidOperationException("Only draft budget plans can be activated.");
        }

        ChangeStatus(BudgetPlanStatus.Active);
    }

    public void Close()
    {
        if (Status == BudgetPlanStatus.Closed)
        {
            throw new InvalidOperationException("Budget plan is already closed.");
        }

        ChangeStatus(BudgetPlanStatus.Closed);
    }

    private void EnsureCanBeModified()
    {
        if (Status != BudgetPlanStatus.Draft)
        {
            throw new InvalidOperationException("Only draft budget plans can be modified.");
        }
    }

    private void EnsureDateIsInsidePeriod(DateOnly expectedDate)
    {
        if (expectedDate < Period.StartDate || expectedDate > Period.EndDate)
        {
            throw new ArgumentOutOfRangeException(nameof(expectedDate), "Planned item date must be inside the budget period.");
        }
    }

    private void EnsureIncomeAmountCanBeUsedForPlan(Money amount, Money? convertedAmount, DateOnly? conversionDate)
    {
        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Planned income amount must be greater than zero.");
        }

        if (amount.Currency.Equals(DefaultCurrency))
        {
            if (convertedAmount is not null)
            {
                throw new ArgumentException(
                    "Converted income amount cannot be provided when income already uses the budget plan default currency.",
                    nameof(convertedAmount));
            }

            if (conversionDate is not null)
            {
                throw new ArgumentException(
                    "Conversion date cannot be provided when income already uses the budget plan default currency.",
                    nameof(conversionDate));
            }

            return;
        }

        if (convertedAmount is null)
        {
            throw new ArgumentException(
                "Converted income amount is required when income currency differs from the budget plan default currency.",
                nameof(convertedAmount));
        }

        if (conversionDate is null)
        {
            throw new ArgumentException(
                "Conversion date is required when income currency differs from the budget plan default currency.",
                nameof(conversionDate));
        }

        if (!convertedAmount.Currency.Equals(DefaultCurrency))
        {
            throw new ArgumentException("Converted income amount must use the budget plan default currency.", nameof(convertedAmount));
        }

        if (convertedAmount.Amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(convertedAmount), "Converted income amount must be greater than zero.");
        }
    }

    private void EnsureCanUseIncomeCategory(BudgetCategory category)
    {
        ArgumentNullException.ThrowIfNull(category);

        if (!category.OwnerId.Equals(OwnerId))
        {
            throw new InvalidOperationException("Budget category belongs to a different owner.");
        }

        if (category.Type != BudgetCategoryType.Income)
        {
            throw new ArgumentException("Only income categories can be used for planned income.", nameof(category));
        }

        if (category.IsArchived)
        {
            throw new InvalidOperationException("Archived budget categories cannot be used for planned income.");
        }
    }

    private void EnsureCanAllocateExpenseCategory(BudgetCategory category)
    {
        ArgumentNullException.ThrowIfNull(category);

        if (!category.OwnerId.Equals(OwnerId))
        {
            throw new InvalidOperationException("Budget category belongs to a different owner.");
        }

        if (category.Type != BudgetCategoryType.Expense)
        {
            throw new ArgumentException("Only expense categories can be allocated.", nameof(category));
        }

        if (category.IsArchived)
        {
            throw new InvalidOperationException("Archived budget categories cannot be allocated.");
        }
    }

    private void EnsureCanUseSavingCategory(BudgetCategory category)
    {
        ArgumentNullException.ThrowIfNull(category);

        if (!category.OwnerId.Equals(OwnerId))
        {
            throw new InvalidOperationException("Budget category belongs to a different owner.");
        }

        if (category.Type != BudgetCategoryType.Saving)
        {
            throw new ArgumentException("Only saving categories can receive saving contributions.", nameof(category));
        }

        if (category.IsArchived)
        {
            throw new InvalidOperationException("Archived budget categories cannot receive saving contributions.");
        }
    }

    private void EnsureAllocationAmountUsesDefaultCurrency(Money amount)
    {
        ArgumentNullException.ThrowIfNull(amount);

        if (!amount.Currency.Equals(DefaultCurrency))
        {
            throw new ArgumentException("Category allocation amount must use the budget plan default currency.", nameof(amount));
        }
    }

    private void EnsureSavingContributionAmountUsesDefaultCurrency(Money amount)
    {
        ArgumentNullException.ThrowIfNull(amount);

        if (!amount.Currency.Equals(DefaultCurrency))
        {
            throw new ArgumentException("Saving contribution amount must use the budget plan default currency.", nameof(amount));
        }
    }

    private void EnsureCategoryHasNoAllocation(BudgetCategoryId categoryId)
    {
        if (_expenseCategoryAllocations.Any(allocation => allocation.CategoryId.Equals(categoryId)))
        {
            throw new InvalidOperationException("Expense category already has an allocation.");
        }
    }

    private void EnsureCategoryHasNoSavingContribution(BudgetCategoryId categoryId)
    {
        if (_savingContributions.Any(contribution => contribution.CategoryId.Equals(categoryId)))
        {
            throw new InvalidOperationException("Saving category already has a contribution.");
        }
    }

    private void EnsurePlannedIncomeIdIsUnique(PlannedIncomeId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (_plannedIncomes.Any(income => income.Id.Equals(id)))
        {
            throw new InvalidOperationException("Planned income id already exists in this budget plan.");
        }
    }

    private void EnsureCategoryAllocationIdIsUnique(CategoryAllocationId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (_expenseCategoryAllocations.Any(allocation => allocation.Id.Equals(id)))
        {
            throw new InvalidOperationException("Category allocation id already exists in this budget plan.");
        }
    }

    private void EnsureSavingContributionIdIsUnique(SavingContributionId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (_savingContributions.Any(contribution => contribution.Id.Equals(id)))
        {
            throw new InvalidOperationException("Saving contribution id already exists in this budget plan.");
        }
    }

    private CategoryAllocation GetExpenseCategoryAllocation(CategoryAllocationId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _expenseCategoryAllocations.SingleOrDefault(allocation => allocation.Id.Equals(id))
            ?? throw new InvalidOperationException("Expense category allocation was not found.");
    }

    private SavingContribution GetSavingContribution(SavingContributionId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _savingContributions.SingleOrDefault(contribution => contribution.Id.Equals(id))
            ?? throw new InvalidOperationException("Saving contribution was not found.");
    }

    private void RecalculateAllocations()
    {
        var totalExpenses = _expenseCategoryAllocations.Sum(allocation => allocation.Amount.Amount);
        var totalSavings = _savingContributions.Sum(contribution => contribution.Amount.Amount);
        var totalIncome = _plannedIncomes.Sum(income => GetIncomeAmountInDefaultCurrency(income).Amount);
        TotalPlannedIncome = new Money(totalIncome, DefaultCurrency);
        TotalAllocatedExpenses = new Money(totalExpenses, DefaultCurrency);
        TotalSavingContributions = new Money(totalSavings, DefaultCurrency);
        PlannedFinancialResult = TotalPlannedIncome - TotalAllocatedExpenses - TotalSavingContributions;

        foreach (var allocation in _expenseCategoryAllocations)
        {
            var expenseShare = totalExpenses == 0m
                ? 0m
                : allocation.Amount.Amount / totalExpenses * 100m;
            var incomeShare = totalIncome == 0m
                ? 0m
                : allocation.Amount.Amount / totalIncome * 100m;

            allocation.UpdatePercentages(expenseShare, incomeShare);
        }

        BudgetFitRisk = CalculateBudgetFitRisk(totalIncome, totalExpenses, totalSavings);
    }

    private static Money GetIncomeAmountInDefaultCurrency(PlannedIncome income)
        => income.ConvertedAmount ?? income.Amount;

    private void ChangeStatus(BudgetPlanStatus status)
    {
        var previousStatus = Status;

        Status = status;
        RaiseDomainEvent(new BudgetPlanStatusChangedEvent(
            Id,
            previousStatus,
            Status,
            DateTimeOffset.UtcNow));
    }

    private BudgetFitRisk CalculateBudgetFitRisk(decimal totalIncome, decimal totalExpenses, decimal totalSavings)
    {
        if (totalExpenses + totalSavings <= totalIncome)
        {
            return BudgetFitRisk.Balanced;
        }

        var fixedExpenses = SumAllocations(CategoryAllocationFlexibility.Fixed);

        if (fixedExpenses > totalIncome)
        {
            return BudgetFitRisk.FixedOverrun;
        }

        var fixedAndFlexibleExpenses = _expenseCategoryAllocations
            .Where(allocation => allocation.Flexibility is CategoryAllocationFlexibility.Fixed or CategoryAllocationFlexibility.Flexible)
            .Sum(allocation => allocation.Amount.Amount);

        return fixedAndFlexibleExpenses > totalIncome
            ? BudgetFitRisk.FlexibleOverrun
            : BudgetFitRisk.OptionalOverrun;
    }

    private decimal SumAllocations(CategoryAllocationFlexibility flexibility)
        => _expenseCategoryAllocations
            .Where(allocation => allocation.Flexibility == flexibility)
            .Sum(allocation => allocation.Amount.Amount);
}
