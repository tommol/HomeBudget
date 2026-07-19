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
    private int _periodYear = 1;
    private int _periodMonth = 1;

    private BudgetPlan()
    {
        OwnerId = null!;
        TotalPlannedIncome = null!;
        TotalAllocatedExpenses = null!;
        TotalSavingContributions = null!;
        PlannedFinancialResult = null!;
        DefaultCurrency = null!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BudgetPlan"/> class.
    /// </summary>
    /// <param name="id">The identifier of the budget plan.</param>
    /// <param name="ownerId">The identifier of the owner of the budget plan.</param>
    /// <param name="period">The period covered by the budget plan.</param>
    /// <param name="defaultCurrency">The default currency used for totals and allocations.</param>
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
        SetPeriod(period);
        DefaultCurrency = defaultCurrency;
        Status = BudgetPlanStatus.Draft;
        BudgetFitRisk = BudgetFitRisk.Balanced;
        TotalPlannedIncome = Money.Zero(defaultCurrency);
        TotalAllocatedExpenses = Money.Zero(defaultCurrency);
        TotalSavingContributions = Money.Zero(defaultCurrency);
        PlannedFinancialResult = Money.Zero(defaultCurrency);
    }

    /// <summary>
    /// Gets the identifier of the owner of the budget plan.
    /// </summary>
    public OwnerId OwnerId { get; private set; }

    /// <summary>
    /// Gets the current status of the budget plan.
    /// </summary>
    public BudgetPlanStatus Status { get; private set; }

    /// <summary>
    /// Gets the current budget fit risk calculated from planned income, expenses, and savings.
    /// </summary>
    public BudgetFitRisk BudgetFitRisk { get; private set; }

    /// <summary>
    /// Gets the total planned income in the default currency.
    /// </summary>
    public Money TotalPlannedIncome { get; private set; }

    /// <summary>
    /// Gets the total allocated expenses in the default currency.
    /// </summary>
    public Money TotalAllocatedExpenses { get; private set; }

    /// <summary>
    /// Gets the total saving contributions in the default currency.
    /// </summary>
    public Money TotalSavingContributions { get; private set; }

    /// <summary>
    /// Gets the planned financial result after expenses and savings are subtracted from income.
    /// </summary>
    public Money PlannedFinancialResult { get; private set; }

    /// <summary>
    /// Gets the period covered by the budget plan.
    /// </summary>
    public BudgetPeriod Period => new(_periodYear, _periodMonth);

    /// <summary>
    /// Gets the default currency used by the budget plan.
    /// </summary>
    public Currency DefaultCurrency { get; private set; }

    /// <summary>
    /// Gets the planned income entries in the budget plan.
    /// </summary>
    public IReadOnlyCollection<PlannedIncome> PlannedIncomes => _plannedIncomes.AsReadOnly();

    /// <summary>
    /// Gets the expense category allocations in the budget plan.
    /// </summary>
    public IReadOnlyCollection<CategoryAllocation> ExpenseCategoryAllocations => _expenseCategoryAllocations.AsReadOnly();

    /// <summary>
    /// Gets the saving contributions in the budget plan.
    /// </summary>
    public IReadOnlyCollection<SavingContribution> SavingContributions => _savingContributions.AsReadOnly();

    /// <summary>
    /// Creates a new budget plan for another period and optionally copies planned entries.
    /// </summary>
    /// <param name="id">The identifier of the copied budget plan.</param>
    /// <param name="period">The period of the copied budget plan.</param>
    /// <param name="plannedIncomeIdFactory">A factory that creates identifiers for copied planned incomes.</param>
    /// <param name="categoryAllocationIdFactory">A factory that creates identifiers for copied category allocations.</param>
    /// <param name="savingContributionIdFactory">A factory that creates identifiers for copied saving contributions.</param>
    /// <param name="copyPlannedIncomes">A value indicating whether planned incomes should be copied.</param>
    /// <param name="copyExpenseCategoryAllocations">A value indicating whether expense category allocations should be copied.</param>
    /// <param name="copySavingContributions">A value indicating whether saving contributions should be copied.</param>
    /// <returns>The copied budget plan.</returns>
    public BudgetPlan CopyTo(
        BudgetPlanId id,
        BudgetPeriod period,
        Func<PlannedIncomeId> plannedIncomeIdFactory,
        Func<CategoryAllocationId> categoryAllocationIdFactory,
        Func<SavingContributionId> savingContributionIdFactory,
        bool copyPlannedIncomes = true,
        bool copyExpenseCategoryAllocations = true,
        bool copySavingContributions = true)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(period);
        ArgumentNullException.ThrowIfNull(plannedIncomeIdFactory);
        ArgumentNullException.ThrowIfNull(categoryAllocationIdFactory);
        ArgumentNullException.ThrowIfNull(savingContributionIdFactory);

        var copy = new BudgetPlan(id, OwnerId, period, DefaultCurrency);

        if (copyPlannedIncomes)
        {
            foreach (var income in _plannedIncomes)
            {
                var plannedIncomeId = plannedIncomeIdFactory();
                copy.EnsurePlannedIncomeIdIsUnique(plannedIncomeId);

                copy._plannedIncomes.Add(new PlannedIncome(
                    plannedIncomeId,
                    income.CategoryId,
                    income.Title,
                    CopyMoney(income.Amount),
                    MoveDateToPeriod(income.ExpectedDate, period),
                    CopyNullableMoney(income.ConvertedAmount),
                    income.ConversionDate is null
                        ? null
                        : MoveDateToPeriod(income.ConversionDate.Value, period)));
            }
        }

        if (copyExpenseCategoryAllocations)
        {
            foreach (var allocation in _expenseCategoryAllocations)
            {
                var allocationId = categoryAllocationIdFactory();
                copy.EnsureCategoryAllocationIdIsUnique(allocationId);

                copy._expenseCategoryAllocations.Add(new CategoryAllocation(
                    allocationId,
                    allocation.CategoryId,
                    CopyMoney(allocation.Amount),
                    allocation.Flexibility));
            }
        }

        if (copySavingContributions)
        {
            foreach (var contribution in _savingContributions)
            {
                var contributionId = savingContributionIdFactory();
                copy.EnsureSavingContributionIdIsUnique(contributionId);

                copy._savingContributions.Add(new SavingContribution(
                    contributionId,
                    contribution.CategoryId,
                    CopyMoney(contribution.Amount)));
            }
        }

        copy.RecalculateAllocations();

        return copy;
    }

    /// <summary>
    /// Adds a planned income entry to the budget plan.
    /// </summary>
    /// <param name="id">The identifier of the planned income.</param>
    /// <param name="category">The income category used by the planned income.</param>
    /// <param name="title">The title of the planned income.</param>
    /// <param name="amount">The planned income amount.</param>
    /// <param name="expectedDate">The date when the income is expected.</param>
    /// <param name="convertedAmount">The income amount converted to the plan default currency, when needed.</param>
    /// <param name="conversionDate">The date of the currency conversion, when needed.</param>
    /// <returns>The added planned income entry.</returns>
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

    /// <summary>
    /// Adds an expense category allocation to the budget plan.
    /// </summary>
    /// <param name="id">The identifier of the category allocation.</param>
    /// <param name="category">The expense category to allocate.</param>
    /// <param name="amount">The allocated amount in the plan default currency.</param>
    /// <param name="flexibility">The flexibility level of the allocation.</param>
    /// <returns>The added category allocation.</returns>
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

    /// <summary>
    /// Changes the amount of an existing expense category allocation.
    /// </summary>
    /// <param name="id">The identifier of the category allocation to update.</param>
    /// <param name="amount">The new amount in the plan default currency.</param>
    public void ChangeExpenseCategoryAllocationAmount(CategoryAllocationId id, Money amount)
    {
        EnsureCanBeModified();
        EnsureAllocationAmountUsesDefaultCurrency(amount);

        var allocation = GetExpenseCategoryAllocation(id);
        var previousAmount = allocation.Amount;

        allocation.ChangeAmount(amount);
        RecalculateAllocations();
        RaiseDomainEvent(new CategoryAllocationAmountChangedEvent(
            Id,
            allocation.Id,
            allocation.CategoryId,
            previousAmount,
            allocation.Amount,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Changes the flexibility level of an existing expense category allocation.
    /// </summary>
    /// <param name="id">The identifier of the category allocation to update.</param>
    /// <param name="flexibility">The new flexibility level.</param>
    public void ChangeExpenseCategoryAllocationFlexibility(CategoryAllocationId id, CategoryAllocationFlexibility flexibility)
    {
        EnsureCanBeModified();

        var allocation = GetExpenseCategoryAllocation(id);
        var previousFlexibility = allocation.Flexibility;

        allocation.ChangeFlexibility(flexibility);
        RecalculateAllocations();
        RaiseDomainEvent(new CategoryAllocationFlexibilityChangedEvent(
            Id,
            allocation.Id,
            allocation.CategoryId,
            previousFlexibility,
            allocation.Flexibility,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Removes an existing expense category allocation.
    /// </summary>
    /// <param name="id">The identifier of the category allocation to remove.</param>
    public void RemoveExpenseCategoryAllocation(CategoryAllocationId id)
    {
        EnsureCanBeModified();

        var allocation = GetExpenseCategoryAllocation(id);

        if (allocation.Flexibility == CategoryAllocationFlexibility.Fixed)
        {
            throw new InvalidOperationException("Fixed expense category allocations cannot be removed.");
        }

        _expenseCategoryAllocations.Remove(allocation);
        RecalculateAllocations();
        RaiseDomainEvent(new CategoryAllocationRemovedEvent(
            Id,
            allocation.Id,
            allocation.CategoryId,
            allocation.Amount,
            allocation.Flexibility,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Adds a saving contribution to the budget plan.
    /// </summary>
    /// <param name="id">The identifier of the saving contribution.</param>
    /// <param name="category">The saving category that receives the contribution.</param>
    /// <param name="amount">The contribution amount in the plan default currency.</param>
    /// <returns>The added saving contribution.</returns>
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

    /// <summary>
    /// Changes the amount of an existing saving contribution.
    /// </summary>
    /// <param name="id">The identifier of the saving contribution to update.</param>
    /// <param name="amount">The new contribution amount in the plan default currency.</param>
    public void ChangeSavingContributionAmount(SavingContributionId id, Money amount)
    {
        EnsureCanBeModified();
        EnsureSavingContributionAmountUsesDefaultCurrency(amount);

        var contribution = GetSavingContribution(id);
        var previousAmount = contribution.Amount;

        contribution.ChangeAmount(amount);
        RecalculateAllocations();
        RaiseDomainEvent(new SavingContributionAmountChangedEvent(
            Id,
            contribution.Id,
            contribution.CategoryId,
            previousAmount,
            contribution.Amount,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Removes an existing saving contribution from the budget plan.
    /// </summary>
    /// <param name="id">The identifier of the saving contribution to remove.</param>
    public void RemoveSavingContribution(SavingContributionId id)
    {
        EnsureCanBeModified();

        var contribution = GetSavingContribution(id);

        _savingContributions.Remove(contribution);
        RecalculateAllocations();
        RaiseDomainEvent(new SavingContributionRemovedEvent(
            Id,
            contribution.Id,
            contribution.CategoryId,
            contribution.Amount,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Activates the budget plan.
    /// </summary>
    public void Activate()
    {
        if (Status != BudgetPlanStatus.Draft)
        {
            throw new InvalidOperationException("Only draft budget plans can be activated.");
        }

        ChangeStatus(BudgetPlanStatus.Active);
    }

    /// <summary>
    /// Closes the budget plan.
    /// </summary>
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

    private void SetPeriod(BudgetPeriod period)
    {
        ArgumentNullException.ThrowIfNull(period);

        _periodYear = period.Year;
        _periodMonth = period.Month;
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

    private static DateOnly MoveDateToPeriod(DateOnly date, BudgetPeriod period)
    {
        var day = Math.Min(date.Day, period.EndDate.Day);

        return new DateOnly(period.Year, period.Month, day);
    }

    private static Money CopyMoney(Money money)
    {
        ArgumentNullException.ThrowIfNull(money);

        return new Money(money.Amount, money.Currency);
    }

    private static Money? CopyNullableMoney(Money? money)
        => money is null ? null : CopyMoney(money);

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
