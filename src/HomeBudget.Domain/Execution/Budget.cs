using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents the executed budget for a period.
/// </summary>
public sealed class Budget : AggregateRoot<BudgetId>
{
    private readonly List<Income> _incomes = [];
    private readonly List<Expense> _expenses = [];
    private readonly List<Saving> _savings = [];
    private int _periodYear = 1;
    private int _periodMonth = 1;

    private Budget()
    {
        OwnerId = null!;
        SourceBudgetPlanId = null!;
        DefaultCurrency = null!;
        TotalIncome = null!;
        TotalExpenses = null!;
        TotalSavings = null!;
        ActualFinancialResult = null!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Budget"/> class.
    /// </summary>
    /// <param name="id">The identifier of the budget.</param>
    /// <param name="ownerId">The identifier of the owner of the budget.</param>
    /// <param name="period">The period covered by the budget.</param>
    /// <param name="defaultCurrency">The default currency used for totals.</param>
    /// <param name="sourceBudgetPlanId">The identifier of the budget plan this budget was created from.</param>
    public Budget(
        BudgetId id,
        OwnerId ownerId,
        BudgetPeriod period,
        Currency defaultCurrency,
        BudgetPlanId sourceBudgetPlanId)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(ownerId);
        ArgumentNullException.ThrowIfNull(period);
        ArgumentNullException.ThrowIfNull(defaultCurrency);
        ArgumentNullException.ThrowIfNull(sourceBudgetPlanId);

        OwnerId = ownerId;
        SourceBudgetPlanId = sourceBudgetPlanId;
        SetPeriod(period);
        DefaultCurrency = defaultCurrency;
        Status = BudgetStatus.Active;
        TotalIncome = Money.Zero(defaultCurrency);
        TotalExpenses = Money.Zero(defaultCurrency);
        TotalSavings = Money.Zero(defaultCurrency);
        ActualFinancialResult = Money.Zero(defaultCurrency);
    }

    /// <summary>
    /// Gets the identifier of the owner of the budget.
    /// </summary>
    public OwnerId OwnerId { get; private set; }

    /// <summary>
    /// Gets the identifier of the budget plan this budget was created from.
    /// </summary>
    public BudgetPlanId SourceBudgetPlanId { get; private set; }

    /// <summary>
    /// Gets the period covered by the budget.
    /// </summary>
    public BudgetPeriod Period => new(_periodYear, _periodMonth);

    /// <summary>
    /// Gets the default currency used by the budget.
    /// </summary>
    public Currency DefaultCurrency { get; private set; }

    /// <summary>
    /// Gets the current status of the budget.
    /// </summary>
    public BudgetStatus Status { get; private set; }

    /// <summary>
    /// Gets the total income in the default currency.
    /// </summary>
    public Money TotalIncome { get; private set; }

    /// <summary>
    /// Gets the total expenses in the default currency.
    /// </summary>
    public Money TotalExpenses { get; private set; }

    /// <summary>
    /// Gets the total savings in the default currency.
    /// </summary>
    public Money TotalSavings { get; private set; }

    /// <summary>
    /// Gets the actual financial result after expenses and savings are subtracted from income.
    /// </summary>
    public Money ActualFinancialResult { get; private set; }

    /// <summary>
    /// Gets the income entries in the budget.
    /// </summary>
    public IReadOnlyCollection<Income> Incomes => _incomes.AsReadOnly();

    /// <summary>
    /// Gets the expense entries in the budget.
    /// </summary>
    public IReadOnlyCollection<Expense> Expenses => _expenses.AsReadOnly();

    /// <summary>
    /// Gets the saving entries in the budget.
    /// </summary>
    public IReadOnlyCollection<Saving> Savings => _savings.AsReadOnly();

    /// <summary>
    /// Adds an income entry to the budget.
    /// </summary>
    /// <param name="id">The identifier of the income.</param>
    /// <param name="category">The income category used by the income.</param>
    /// <param name="title">The title of the income.</param>
    /// <param name="amount">The income amount.</param>
    /// <param name="occurredDate">The date when the income occurred.</param>
    /// <param name="convertedAmount">The income amount converted to the budget default currency, when needed.</param>
    /// <param name="conversionDate">The date of the currency conversion, when needed.</param>
    /// <returns>The added income entry.</returns>
    public Income AddIncome(
        IncomeId id,
        BudgetCategory category,
        string title,
        Money amount,
        DateOnly occurredDate,
        Money? convertedAmount = null,
        DateOnly? conversionDate = null)
    {
        EnsureCanBeModified();
        EnsureIncomeIdIsUnique(id);
        EnsureCanUseIncomeCategory(category);
        EnsureDateIsInsidePeriod(occurredDate);
        EnsureAmountCanBeUsedInBudget(amount, convertedAmount, conversionDate, "income");

        var income = new Income(id, category.Id, title, amount, occurredDate, convertedAmount, conversionDate);

        _incomes.Add(income);
        RecalculateTotals();
        RaiseDomainEvent(new IncomeAddedEvent(
            Id,
            income.Id,
            income.CategoryId,
            income.Title,
            income.Amount,
            income.ConvertedAmount,
            income.ConversionDate,
            income.OccurredDate,
            DateTimeOffset.UtcNow));

        return income;
    }

    /// <summary>
    /// Adds an expense entry to the budget.
    /// </summary>
    /// <param name="id">The identifier of the expense.</param>
    /// <param name="category">The expense category used by the expense.</param>
    /// <param name="title">The title of the expense.</param>
    /// <param name="amount">The expense amount.</param>
    /// <param name="occurredDate">The date when the expense occurred.</param>
    /// <param name="convertedAmount">The expense amount converted to the budget default currency, when needed.</param>
    /// <param name="conversionDate">The date of the currency conversion, when needed.</param>
    /// <returns>The added expense entry.</returns>
    public Expense AddExpense(
        ExpenseId id,
        BudgetCategory category,
        string title,
        Money amount,
        DateOnly occurredDate,
        Money? convertedAmount = null,
        DateOnly? conversionDate = null)
    {
        EnsureCanBeModified();
        EnsureExpenseIdIsUnique(id);
        EnsureCanUseExpenseCategory(category);
        EnsureDateIsInsidePeriod(occurredDate);
        EnsureAmountCanBeUsedInBudget(amount, convertedAmount, conversionDate, "expense");

        var expense = new Expense(id, category.Id, title, amount, occurredDate, convertedAmount, conversionDate);

        _expenses.Add(expense);
        RecalculateTotals();
        RaiseDomainEvent(new ExpenseAddedEvent(
            Id,
            expense.Id,
            expense.CategoryId,
            expense.Title,
            expense.Amount,
            expense.ConvertedAmount,
            expense.ConversionDate,
            expense.OccurredDate,
            DateTimeOffset.UtcNow));

        return expense;
    }

    /// <summary>
    /// Adds a saving entry to the budget.
    /// </summary>
    /// <param name="id">The identifier of the saving.</param>
    /// <param name="category">The saving category used by the saving.</param>
    /// <param name="title">The title of the saving.</param>
    /// <param name="amount">The saving amount.</param>
    /// <param name="occurredDate">The date when the saving occurred.</param>
    /// <param name="convertedAmount">The saving amount converted to the budget default currency, when needed.</param>
    /// <param name="conversionDate">The date of the currency conversion, when needed.</param>
    /// <returns>The added saving entry.</returns>
    public Saving AddSaving(
        SavingId id,
        BudgetCategory category,
        string title,
        Money amount,
        DateOnly occurredDate,
        Money? convertedAmount = null,
        DateOnly? conversionDate = null)
    {
        EnsureCanBeModified();
        EnsureSavingIdIsUnique(id);
        EnsureCanUseSavingCategory(category);
        EnsureDateIsInsidePeriod(occurredDate);
        EnsureAmountCanBeUsedInBudget(amount, convertedAmount, conversionDate, "saving");

        var saving = new Saving(id, category.Id, title, amount, occurredDate, convertedAmount, conversionDate);

        _savings.Add(saving);
        RecalculateTotals();
        RaiseDomainEvent(new SavingAddedEvent(
            Id,
            saving.Id,
            saving.CategoryId,
            saving.Title,
            saving.Amount,
            saving.ConvertedAmount,
            saving.ConversionDate,
            saving.OccurredDate,
            DateTimeOffset.UtcNow));

        return saving;
    }

    /// <summary>
    /// Changes the amount of an existing income entry.
    /// </summary>
    /// <param name="id">The identifier of the income to update.</param>
    /// <param name="amount">The corrected income amount.</param>
    /// <param name="convertedAmount">The corrected amount converted to the budget default currency, when needed.</param>
    /// <param name="conversionDate">The date of the currency conversion, when needed.</param>
    public void ChangeIncomeAmount(
        IncomeId id,
        Money amount,
        Money? convertedAmount = null,
        DateOnly? conversionDate = null)
    {
        EnsureCanBeModified();
        EnsureAmountCanBeUsedInBudget(amount, convertedAmount, conversionDate, "income");

        var income = GetIncome(id);
        EnsureCanModifyIncome(income);
        var previousAmount = income.Amount;
        var previousConvertedAmount = income.ConvertedAmount;
        var previousConversionDate = income.ConversionDate;

        income.ChangeAmount(amount, convertedAmount, conversionDate);
        RecalculateTotals();
        RaiseDomainEvent(new IncomeAmountChangedEvent(
            Id,
            income.Id,
            income.CategoryId,
            previousAmount,
            income.Amount,
            previousConvertedAmount,
            income.ConvertedAmount,
            previousConversionDate,
            income.ConversionDate,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Changes the category of an existing income entry.
    /// </summary>
    /// <param name="id">The identifier of the income to update.</param>
    /// <param name="category">The corrected income category.</param>
    public void ChangeIncomeCategory(IncomeId id, BudgetCategory category)
    {
        EnsureCanBeModified();
        EnsureCanUseIncomeCategory(category);

        var income = GetIncome(id);
        EnsureCanModifyIncome(income);
        var previousCategoryId = income.CategoryId;

        income.ChangeCategory(category.Id);
        RaiseDomainEvent(new IncomeCategoryChangedEvent(
            Id,
            income.Id,
            previousCategoryId,
            income.CategoryId,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Changes the title of an existing income entry.
    /// </summary>
    /// <param name="id">The identifier of the income to update.</param>
    /// <param name="title">The corrected income title.</param>
    public void ChangeIncomeTitle(IncomeId id, string title)
    {
        EnsureCanBeModified();

        var income = GetIncome(id);
        EnsureCanModifyIncome(income);
        var previousTitle = income.Title;

        income.ChangeTitle(title);
        RaiseDomainEvent(new IncomeTitleChangedEvent(
            Id,
            income.Id,
            income.CategoryId,
            previousTitle,
            income.Title,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Changes the occurred date of an existing income entry.
    /// </summary>
    /// <param name="id">The identifier of the income to update.</param>
    /// <param name="occurredDate">The corrected occurred date.</param>
    public void ChangeIncomeOccurredDate(IncomeId id, DateOnly occurredDate)
    {
        EnsureCanBeModified();
        EnsureDateIsInsidePeriod(occurredDate);

        var income = GetIncome(id);
        EnsureCanModifyIncome(income);
        var previousOccurredDate = income.OccurredDate;

        income.ChangeOccurredDate(occurredDate);
        RaiseDomainEvent(new IncomeOccurredDateChangedEvent(
            Id,
            income.Id,
            income.CategoryId,
            previousOccurredDate,
            income.OccurredDate,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Changes the amount of an existing expense entry.
    /// </summary>
    /// <param name="id">The identifier of the expense to update.</param>
    /// <param name="amount">The corrected expense amount.</param>
    /// <param name="convertedAmount">The corrected amount converted to the budget default currency, when needed.</param>
    /// <param name="conversionDate">The date of the currency conversion, when needed.</param>
    public void ChangeExpenseAmount(
        ExpenseId id,
        Money amount,
        Money? convertedAmount = null,
        DateOnly? conversionDate = null)
    {
        EnsureCanBeModified();
        EnsureAmountCanBeUsedInBudget(amount, convertedAmount, conversionDate, "expense");

        var expense = GetExpense(id);
        EnsureCanModifyExpense(expense);
        var previousAmount = expense.Amount;
        var previousConvertedAmount = expense.ConvertedAmount;
        var previousConversionDate = expense.ConversionDate;

        expense.ChangeAmount(amount, convertedAmount, conversionDate);
        RecalculateTotals();
        RaiseDomainEvent(new ExpenseAmountChangedEvent(
            Id,
            expense.Id,
            expense.CategoryId,
            previousAmount,
            expense.Amount,
            previousConvertedAmount,
            expense.ConvertedAmount,
            previousConversionDate,
            expense.ConversionDate,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Changes the category of an existing expense entry.
    /// </summary>
    /// <param name="id">The identifier of the expense to update.</param>
    /// <param name="category">The corrected expense category.</param>
    public void ChangeExpenseCategory(ExpenseId id, BudgetCategory category)
    {
        EnsureCanBeModified();
        EnsureCanUseExpenseCategory(category);

        var expense = GetExpense(id);
        EnsureCanModifyExpense(expense);
        var previousCategoryId = expense.CategoryId;

        expense.ChangeCategory(category.Id);
        RaiseDomainEvent(new ExpenseCategoryChangedEvent(
            Id,
            expense.Id,
            previousCategoryId,
            expense.CategoryId,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Changes the title of an existing expense entry.
    /// </summary>
    /// <param name="id">The identifier of the expense to update.</param>
    /// <param name="title">The corrected expense title.</param>
    public void ChangeExpenseTitle(ExpenseId id, string title)
    {
        EnsureCanBeModified();

        var expense = GetExpense(id);
        EnsureCanModifyExpense(expense);
        var previousTitle = expense.Title;

        expense.ChangeTitle(title);
        RaiseDomainEvent(new ExpenseTitleChangedEvent(
            Id,
            expense.Id,
            expense.CategoryId,
            previousTitle,
            expense.Title,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Changes the occurred date of an existing expense entry.
    /// </summary>
    /// <param name="id">The identifier of the expense to update.</param>
    /// <param name="occurredDate">The corrected occurred date.</param>
    public void ChangeExpenseOccurredDate(ExpenseId id, DateOnly occurredDate)
    {
        EnsureCanBeModified();
        EnsureDateIsInsidePeriod(occurredDate);

        var expense = GetExpense(id);
        EnsureCanModifyExpense(expense);
        var previousOccurredDate = expense.OccurredDate;

        expense.ChangeOccurredDate(occurredDate);
        RaiseDomainEvent(new ExpenseOccurredDateChangedEvent(
            Id,
            expense.Id,
            expense.CategoryId,
            previousOccurredDate,
            expense.OccurredDate,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Changes the amount of an existing saving entry.
    /// </summary>
    /// <param name="id">The identifier of the saving to update.</param>
    /// <param name="amount">The corrected saving amount.</param>
    /// <param name="convertedAmount">The corrected amount converted to the budget default currency, when needed.</param>
    /// <param name="conversionDate">The date of the currency conversion, when needed.</param>
    public void ChangeSavingAmount(
        SavingId id,
        Money amount,
        Money? convertedAmount = null,
        DateOnly? conversionDate = null)
    {
        EnsureCanBeModified();
        EnsureAmountCanBeUsedInBudget(amount, convertedAmount, conversionDate, "saving");

        var saving = GetSaving(id);
        EnsureCanModifySaving(saving);
        var previousAmount = saving.Amount;
        var previousConvertedAmount = saving.ConvertedAmount;
        var previousConversionDate = saving.ConversionDate;

        saving.ChangeAmount(amount, convertedAmount, conversionDate);
        RecalculateTotals();
        RaiseDomainEvent(new SavingAmountChangedEvent(
            Id,
            saving.Id,
            saving.CategoryId,
            previousAmount,
            saving.Amount,
            previousConvertedAmount,
            saving.ConvertedAmount,
            previousConversionDate,
            saving.ConversionDate,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Changes the category of an existing saving entry.
    /// </summary>
    /// <param name="id">The identifier of the saving to update.</param>
    /// <param name="category">The corrected saving category.</param>
    public void ChangeSavingCategory(SavingId id, BudgetCategory category)
    {
        EnsureCanBeModified();
        EnsureCanUseSavingCategory(category);

        var saving = GetSaving(id);
        EnsureCanModifySaving(saving);
        var previousCategoryId = saving.CategoryId;

        saving.ChangeCategory(category.Id);
        RaiseDomainEvent(new SavingCategoryChangedEvent(
            Id,
            saving.Id,
            previousCategoryId,
            saving.CategoryId,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Changes the title of an existing saving entry.
    /// </summary>
    /// <param name="id">The identifier of the saving to update.</param>
    /// <param name="title">The corrected saving title.</param>
    public void ChangeSavingTitle(SavingId id, string title)
    {
        EnsureCanBeModified();

        var saving = GetSaving(id);
        EnsureCanModifySaving(saving);
        var previousTitle = saving.Title;

        saving.ChangeTitle(title);
        RaiseDomainEvent(new SavingTitleChangedEvent(
            Id,
            saving.Id,
            saving.CategoryId,
            previousTitle,
            saving.Title,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Changes the occurred date of an existing saving entry.
    /// </summary>
    /// <param name="id">The identifier of the saving to update.</param>
    /// <param name="occurredDate">The corrected occurred date.</param>
    public void ChangeSavingOccurredDate(SavingId id, DateOnly occurredDate)
    {
        EnsureCanBeModified();
        EnsureDateIsInsidePeriod(occurredDate);

        var saving = GetSaving(id);
        EnsureCanModifySaving(saving);
        var previousOccurredDate = saving.OccurredDate;

        saving.ChangeOccurredDate(occurredDate);
        RaiseDomainEvent(new SavingOccurredDateChangedEvent(
            Id,
            saving.Id,
            saving.CategoryId,
            previousOccurredDate,
            saving.OccurredDate,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Soft-removes an existing income entry from budget totals.
    /// </summary>
    /// <param name="id">The identifier of the income to remove.</param>
    /// <param name="removalReason">The reason why the income was removed.</param>
    public void RemoveIncome(IncomeId id, string removalReason)
    {
        EnsureCanBeModified();

        var income = GetIncome(id);
        EnsureCanModifyIncome(income);
        var removedOnUtc = DateTimeOffset.UtcNow;

        income.Remove(removalReason, removedOnUtc);
        RecalculateTotals();
        RaiseDomainEvent(new IncomeRemovedEvent(
            Id,
            income.Id,
            income.CategoryId,
            income.Title,
            income.Amount,
            income.ConvertedAmount,
            income.ConversionDate,
            income.OccurredDate,
            income.RemovalReason!,
            income.RemovedOnUtc!.Value));
    }

    /// <summary>
    /// Soft-removes an existing expense entry from budget totals.
    /// </summary>
    /// <param name="id">The identifier of the expense to remove.</param>
    /// <param name="removalReason">The reason why the expense was removed.</param>
    public void RemoveExpense(ExpenseId id, string removalReason)
    {
        EnsureCanBeModified();

        var expense = GetExpense(id);
        EnsureCanModifyExpense(expense);
        var removedOnUtc = DateTimeOffset.UtcNow;

        expense.Remove(removalReason, removedOnUtc);
        RecalculateTotals();
        RaiseDomainEvent(new ExpenseRemovedEvent(
            Id,
            expense.Id,
            expense.CategoryId,
            expense.Title,
            expense.Amount,
            expense.ConvertedAmount,
            expense.ConversionDate,
            expense.OccurredDate,
            expense.RemovalReason!,
            expense.RemovedOnUtc!.Value));
    }

    /// <summary>
    /// Soft-removes an existing saving entry from budget totals.
    /// </summary>
    /// <param name="id">The identifier of the saving to remove.</param>
    /// <param name="removalReason">The reason why the saving was removed.</param>
    public void RemoveSaving(SavingId id, string removalReason)
    {
        EnsureCanBeModified();

        var saving = GetSaving(id);
        EnsureCanModifySaving(saving);
        var removedOnUtc = DateTimeOffset.UtcNow;

        saving.Remove(removalReason, removedOnUtc);
        RecalculateTotals();
        RaiseDomainEvent(new SavingRemovedEvent(
            Id,
            saving.Id,
            saving.CategoryId,
            saving.Title,
            saving.Amount,
            saving.ConvertedAmount,
            saving.ConversionDate,
            saving.OccurredDate,
            saving.RemovalReason!,
            saving.RemovedOnUtc!.Value));
    }

    /// <summary>
    /// Closes the budget.
    /// </summary>
    public void Close()
    {
        if (Status == BudgetStatus.Closed)
        {
            throw new InvalidOperationException("Budget is already closed.");
        }

        ChangeStatus(BudgetStatus.Closed);
    }

    private void EnsureCanBeModified()
    {
        if (Status != BudgetStatus.Active)
        {
            throw new InvalidOperationException("Only active budgets can be modified.");
        }
    }

    private void SetPeriod(BudgetPeriod period)
    {
        ArgumentNullException.ThrowIfNull(period);

        _periodYear = period.Year;
        _periodMonth = period.Month;
    }

    private void EnsureDateIsInsidePeriod(DateOnly occurredDate)
    {
        if (occurredDate < Period.StartDate || occurredDate > Period.EndDate)
        {
            throw new ArgumentOutOfRangeException(nameof(occurredDate), "Budget item date must be inside the budget period.");
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
            throw new ArgumentException("Only income categories can be used for income.", nameof(category));
        }

        if (category.IsArchived)
        {
            throw new InvalidOperationException("Archived budget categories cannot be used for income.");
        }
    }

    private void EnsureCanUseExpenseCategory(BudgetCategory category)
    {
        ArgumentNullException.ThrowIfNull(category);

        if (!category.OwnerId.Equals(OwnerId))
        {
            throw new InvalidOperationException("Budget category belongs to a different owner.");
        }

        if (category.Type != BudgetCategoryType.Expense)
        {
            throw new ArgumentException("Only expense categories can be used for expenses.", nameof(category));
        }

        if (category.IsArchived)
        {
            throw new InvalidOperationException("Archived budget categories cannot be used for expenses.");
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
            throw new ArgumentException("Only saving categories can be used for savings.", nameof(category));
        }

        if (category.IsArchived)
        {
            throw new InvalidOperationException("Archived budget categories cannot be used for savings.");
        }
    }

    private void EnsureAmountCanBeUsedInBudget(
        Money amount,
        Money? convertedAmount,
        DateOnly? conversionDate,
        string itemName)
    {
        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), $"Budget {itemName} amount must be greater than zero.");
        }

        if (amount.Currency.Equals(DefaultCurrency))
        {
            if (convertedAmount is not null)
            {
                throw new ArgumentException(
                    $"Converted {itemName} amount cannot be provided when {itemName} already uses the budget default currency.",
                    nameof(convertedAmount));
            }

            if (conversionDate is not null)
            {
                throw new ArgumentException(
                    $"Conversion date cannot be provided when {itemName} already uses the budget default currency.",
                    nameof(conversionDate));
            }

            return;
        }

        if (convertedAmount is null)
        {
            throw new ArgumentException(
                $"Converted {itemName} amount is required when {itemName} currency differs from the budget default currency.",
                nameof(convertedAmount));
        }

        if (conversionDate is null)
        {
            throw new ArgumentException(
                $"Conversion date is required when {itemName} currency differs from the budget default currency.",
                nameof(conversionDate));
        }

        if (!convertedAmount.Currency.Equals(DefaultCurrency))
        {
            throw new ArgumentException($"Converted {itemName} amount must use the budget default currency.", nameof(convertedAmount));
        }

        if (convertedAmount.Amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(convertedAmount), $"Converted {itemName} amount must be greater than zero.");
        }
    }

    private void EnsureIncomeIdIsUnique(IncomeId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (_incomes.Any(income => income.Id.Equals(id)))
        {
            throw new InvalidOperationException("Income id already exists in this budget.");
        }
    }

    private void EnsureExpenseIdIsUnique(ExpenseId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (_expenses.Any(expense => expense.Id.Equals(id)))
        {
            throw new InvalidOperationException("Expense id already exists in this budget.");
        }
    }

    private void EnsureSavingIdIsUnique(SavingId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (_savings.Any(saving => saving.Id.Equals(id)))
        {
            throw new InvalidOperationException("Saving id already exists in this budget.");
        }
    }

    private Income GetIncome(IncomeId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _incomes.SingleOrDefault(income => income.Id.Equals(id))
            ?? throw new InvalidOperationException("Income was not found.");
    }

    private Expense GetExpense(ExpenseId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _expenses.SingleOrDefault(expense => expense.Id.Equals(id))
            ?? throw new InvalidOperationException("Expense was not found.");
    }

    private Saving GetSaving(SavingId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _savings.SingleOrDefault(saving => saving.Id.Equals(id))
            ?? throw new InvalidOperationException("Saving was not found.");
    }

    private static void EnsureCanModifyIncome(Income income)
    {
        if (income.IsRemoved)
        {
            throw new InvalidOperationException("Removed income entries cannot be modified.");
        }
    }

    private static void EnsureCanModifyExpense(Expense expense)
    {
        if (expense.IsRemoved)
        {
            throw new InvalidOperationException("Removed expense entries cannot be modified.");
        }
    }

    private static void EnsureCanModifySaving(Saving saving)
    {
        if (saving.IsRemoved)
        {
            throw new InvalidOperationException("Removed saving entries cannot be modified.");
        }
    }

    private void RecalculateTotals()
    {
        TotalIncome = new Money(
            _incomes
                .Where(income => !income.IsRemoved)
                .Sum(income => GetAmountInDefaultCurrency(income.Amount, income.ConvertedAmount).Amount),
            DefaultCurrency);
        TotalExpenses = new Money(
            _expenses
                .Where(expense => !expense.IsRemoved)
                .Sum(expense => GetAmountInDefaultCurrency(expense.Amount, expense.ConvertedAmount).Amount),
            DefaultCurrency);
        TotalSavings = new Money(
            _savings
                .Where(saving => !saving.IsRemoved)
                .Sum(saving => GetAmountInDefaultCurrency(saving.Amount, saving.ConvertedAmount).Amount),
            DefaultCurrency);
        ActualFinancialResult = TotalIncome - TotalExpenses - TotalSavings;
    }

    private static Money GetAmountInDefaultCurrency(Money amount, Money? convertedAmount)
        => convertedAmount ?? amount;

    private void ChangeStatus(BudgetStatus status)
    {
        var previousStatus = Status;

        Status = status;
        RaiseDomainEvent(new BudgetStatusChangedEvent(
            Id,
            previousStatus,
            Status,
            DateTimeOffset.UtcNow));
    }
}
