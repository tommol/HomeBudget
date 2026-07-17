using HomeBudget.Domain.Execution;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Tests.Execution;

public sealed class BudgetTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var id = new BudgetId(Guid.NewGuid());
        var ownerId = new OwnerId(Guid.NewGuid());
        var period = new BudgetPeriod(2026, 7);
        var defaultCurrency = Currency.PLN;

        var budget = new Budget(id, ownerId, period, defaultCurrency);

        Assert.Equal(id, budget.Id);
        Assert.Equal(ownerId, budget.OwnerId);
        Assert.Equal(period, budget.Period);
        Assert.Equal(defaultCurrency, budget.DefaultCurrency);
        Assert.Equal(BudgetStatus.Active, budget.Status);
        Assert.Equal(Money.Zero(defaultCurrency), budget.TotalIncome);
        Assert.Equal(Money.Zero(defaultCurrency), budget.TotalExpenses);
        Assert.Equal(Money.Zero(defaultCurrency), budget.TotalSavings);
        Assert.Equal(Money.Zero(defaultCurrency), budget.ActualFinancialResult);
        Assert.Empty(budget.Incomes);
        Assert.Empty(budget.Expenses);
        Assert.Empty(budget.Savings);
    }

    [Fact]
    public void AddIncome_AddsIncomeAndRecalculatesTotal()
    {
        var budget = CreateBudget();
        var incomeId = new IncomeId(Guid.NewGuid());
        var category = CreateIncomeCategory(budget.OwnerId);
        var amount = new Money(5000m, Currency.PLN);
        var occurredDate = new DateOnly(2026, 7, 10);

        var income = budget.AddIncome(incomeId, category, " Salary ", amount, occurredDate);

        Assert.Equal(incomeId, income.Id);
        Assert.Equal(category.Id, income.CategoryId);
        Assert.Equal("Salary", income.Title);
        Assert.Equal(amount, income.Amount);
        Assert.Equal(occurredDate, income.OccurredDate);
        Assert.Null(income.ConvertedAmount);
        Assert.Null(income.ConversionDate);
        Assert.Equal(BudgetEntryStatus.Active, income.Status);
        Assert.False(income.IsRemoved);
        Assert.Null(income.RemovalReason);
        Assert.Null(income.RemovedOnUtc);
        Assert.Contains(income, budget.Incomes);
        Assert.Equal(new Money(5000m, Currency.PLN), budget.TotalIncome);
        Assert.Equal(new Money(5000m, Currency.PLN), budget.ActualFinancialResult);
    }

    [Fact]
    public void AddIncome_RaisesIncomeAddedEvent()
    {
        var budget = CreateBudget();
        var incomeId = new IncomeId(Guid.NewGuid());
        var category = CreateIncomeCategory(budget.OwnerId);
        var title = " Salary ";
        var amount = new Money(1000m, Currency.EUR);
        var convertedAmount = new Money(4250m, Currency.PLN);
        var occurredDate = new DateOnly(2026, 7, 10);
        var conversionDate = new DateOnly(2026, 7, 9);
        var before = DateTimeOffset.UtcNow;

        budget.AddIncome(
            incomeId,
            category,
            title,
            amount,
            occurredDate,
            convertedAmount,
            conversionDate);

        var after = DateTimeOffset.UtcNow;
        var domainEvent = Assert.IsType<IncomeAddedEvent>(Assert.Single(budget.DomainEvents));
        Assert.Equal(budget.Id, domainEvent.BudgetId);
        Assert.Equal(incomeId, domainEvent.IncomeId);
        Assert.Equal(category.Id, domainEvent.CategoryId);
        Assert.Equal("Salary", domainEvent.Title);
        Assert.Equal(amount, domainEvent.Amount);
        Assert.Equal(convertedAmount, domainEvent.ConvertedAmount);
        Assert.Equal(conversionDate, domainEvent.ConversionDate);
        Assert.Equal(occurredDate, domainEvent.OccurredDate);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void AddIncome_AddsConvertedAmount()
    {
        var budget = CreateBudget();
        var amount = new Money(1000m, Currency.EUR);
        var convertedAmount = new Money(4250m, Currency.PLN);
        var conversionDate = new DateOnly(2026, 7, 9);

        var income = budget.AddIncome(
            new IncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budget.OwnerId),
            "Salary",
            amount,
            new DateOnly(2026, 7, 10),
            convertedAmount,
            conversionDate);

        Assert.Equal(amount, income.Amount);
        Assert.Equal(convertedAmount, income.ConvertedAmount);
        Assert.Equal(conversionDate, income.ConversionDate);
        Assert.Equal(new Money(4250m, Currency.PLN), budget.TotalIncome);
    }

    [Fact]
    public void AddExpense_AddsExpenseAndRecalculatesTotal()
    {
        var budget = CreateBudget();
        var expenseId = new ExpenseId(Guid.NewGuid());
        var category = CreateExpenseCategory(budget.OwnerId);
        var amount = new Money(250m, Currency.PLN);
        var occurredDate = new DateOnly(2026, 7, 12);

        var expense = budget.AddExpense(expenseId, category, " Groceries ", amount, occurredDate);

        Assert.Equal(expenseId, expense.Id);
        Assert.Equal(category.Id, expense.CategoryId);
        Assert.Equal("Groceries", expense.Title);
        Assert.Equal(amount, expense.Amount);
        Assert.Equal(occurredDate, expense.OccurredDate);
        Assert.Null(expense.ConvertedAmount);
        Assert.Null(expense.ConversionDate);
        Assert.Equal(BudgetEntryStatus.Active, expense.Status);
        Assert.False(expense.IsRemoved);
        Assert.Null(expense.RemovalReason);
        Assert.Null(expense.RemovedOnUtc);
        Assert.Contains(expense, budget.Expenses);
        Assert.Equal(new Money(250m, Currency.PLN), budget.TotalExpenses);
        Assert.Equal(new Money(-250m, Currency.PLN), budget.ActualFinancialResult);
    }

    [Fact]
    public void AddExpense_RaisesExpenseAddedEvent()
    {
        var budget = CreateBudget();
        var expenseId = new ExpenseId(Guid.NewGuid());
        var category = CreateExpenseCategory(budget.OwnerId);
        var title = " Groceries ";
        var amount = new Money(250m, Currency.PLN);
        var occurredDate = new DateOnly(2026, 7, 12);
        var before = DateTimeOffset.UtcNow;

        budget.AddExpense(expenseId, category, title, amount, occurredDate);

        var after = DateTimeOffset.UtcNow;
        var domainEvent = Assert.IsType<ExpenseAddedEvent>(Assert.Single(budget.DomainEvents));
        Assert.Equal(budget.Id, domainEvent.BudgetId);
        Assert.Equal(expenseId, domainEvent.ExpenseId);
        Assert.Equal(category.Id, domainEvent.CategoryId);
        Assert.Equal("Groceries", domainEvent.Title);
        Assert.Equal(amount, domainEvent.Amount);
        Assert.Null(domainEvent.ConvertedAmount);
        Assert.Null(domainEvent.ConversionDate);
        Assert.Equal(occurredDate, domainEvent.OccurredDate);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void AddSaving_AddsSavingAndRecalculatesTotal()
    {
        var budget = CreateBudget();
        var savingId = new SavingId(Guid.NewGuid());
        var category = CreateSavingCategory(budget.OwnerId);
        var amount = new Money(1000m, Currency.PLN);
        var occurredDate = new DateOnly(2026, 7, 15);

        var saving = budget.AddSaving(savingId, category, " Emergency fund ", amount, occurredDate);

        Assert.Equal(savingId, saving.Id);
        Assert.Equal(category.Id, saving.CategoryId);
        Assert.Equal("Emergency fund", saving.Title);
        Assert.Equal(amount, saving.Amount);
        Assert.Equal(occurredDate, saving.OccurredDate);
        Assert.Null(saving.ConvertedAmount);
        Assert.Null(saving.ConversionDate);
        Assert.Equal(BudgetEntryStatus.Active, saving.Status);
        Assert.False(saving.IsRemoved);
        Assert.Null(saving.RemovalReason);
        Assert.Null(saving.RemovedOnUtc);
        Assert.Contains(saving, budget.Savings);
        Assert.Equal(new Money(1000m, Currency.PLN), budget.TotalSavings);
        Assert.Equal(new Money(-1000m, Currency.PLN), budget.ActualFinancialResult);
    }

    [Fact]
    public void AddSaving_AddsConvertedAmount()
    {
        var budget = CreateBudget();
        var amount = new Money(500m, Currency.EUR);
        var convertedAmount = new Money(2125m, Currency.PLN);
        var conversionDate = new DateOnly(2026, 7, 15);

        var saving = budget.AddSaving(
            new SavingId(Guid.NewGuid()),
            CreateSavingCategory(budget.OwnerId),
            "Broker transfer",
            amount,
            new DateOnly(2026, 7, 16),
            convertedAmount,
            conversionDate);

        Assert.Equal(amount, saving.Amount);
        Assert.Equal(convertedAmount, saving.ConvertedAmount);
        Assert.Equal(conversionDate, saving.ConversionDate);
        Assert.Equal(new Money(2125m, Currency.PLN), budget.TotalSavings);
    }

    [Fact]
    public void AddSaving_RaisesSavingAddedEvent()
    {
        var budget = CreateBudget();
        var savingId = new SavingId(Guid.NewGuid());
        var category = CreateSavingCategory(budget.OwnerId);
        var title = " Emergency fund ";
        var amount = new Money(1000m, Currency.PLN);
        var occurredDate = new DateOnly(2026, 7, 15);
        var before = DateTimeOffset.UtcNow;

        budget.AddSaving(savingId, category, title, amount, occurredDate);

        var after = DateTimeOffset.UtcNow;
        var domainEvent = Assert.IsType<SavingAddedEvent>(Assert.Single(budget.DomainEvents));
        Assert.Equal(budget.Id, domainEvent.BudgetId);
        Assert.Equal(savingId, domainEvent.SavingId);
        Assert.Equal(category.Id, domainEvent.CategoryId);
        Assert.Equal("Emergency fund", domainEvent.Title);
        Assert.Equal(amount, domainEvent.Amount);
        Assert.Null(domainEvent.ConvertedAmount);
        Assert.Null(domainEvent.ConversionDate);
        Assert.Equal(occurredDate, domainEvent.OccurredDate);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void ChangeIncomeAmount_ChangesAmountAndRecalculatesTotal()
    {
        var budget = CreateBudget();
        var income = budget.AddIncome(
            new IncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budget.OwnerId),
            "Salary",
            new Money(1000m, Currency.EUR),
            new DateOnly(2026, 7, 10),
            new Money(4250m, Currency.PLN),
            new DateOnly(2026, 7, 9));
        var newAmount = new Money(1200m, Currency.EUR);
        var newConvertedAmount = new Money(5100m, Currency.PLN);
        var newConversionDate = new DateOnly(2026, 7, 11);

        budget.ChangeIncomeAmount(income.Id, newAmount, newConvertedAmount, newConversionDate);

        Assert.Equal(newAmount, income.Amount);
        Assert.Equal(newConvertedAmount, income.ConvertedAmount);
        Assert.Equal(newConversionDate, income.ConversionDate);
        Assert.Equal(new Money(5100m, Currency.PLN), budget.TotalIncome);
        Assert.Equal(new Money(5100m, Currency.PLN), budget.ActualFinancialResult);
    }

    [Fact]
    public void ChangeIncomeAmount_RaisesIncomeAmountChangedEvent()
    {
        var budget = CreateBudget();
        var income = budget.AddIncome(
            new IncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budget.OwnerId),
            "Salary",
            new Money(1000m, Currency.PLN),
            new DateOnly(2026, 7, 10));
        var newAmount = new Money(1500m, Currency.PLN);
        budget.ClearDomainEvents();
        var before = DateTimeOffset.UtcNow;

        budget.ChangeIncomeAmount(income.Id, newAmount);

        var after = DateTimeOffset.UtcNow;
        var domainEvent = Assert.IsType<IncomeAmountChangedEvent>(Assert.Single(budget.DomainEvents));
        Assert.Equal(budget.Id, domainEvent.BudgetId);
        Assert.Equal(income.Id, domainEvent.IncomeId);
        Assert.Equal(income.CategoryId, domainEvent.CategoryId);
        Assert.Equal(new Money(1000m, Currency.PLN), domainEvent.PreviousAmount);
        Assert.Equal(newAmount, domainEvent.NewAmount);
        Assert.Null(domainEvent.PreviousConvertedAmount);
        Assert.Null(domainEvent.NewConvertedAmount);
        Assert.Null(domainEvent.PreviousConversionDate);
        Assert.Null(domainEvent.NewConversionDate);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void ChangeExpenseAmount_ChangesAmountAndRecalculatesTotal()
    {
        var budget = CreateBudget();
        var expense = budget.AddExpense(
            new ExpenseId(Guid.NewGuid()),
            CreateExpenseCategory(budget.OwnerId),
            "Groceries",
            new Money(250m, Currency.PLN),
            new DateOnly(2026, 7, 12));
        var newAmount = new Money(300m, Currency.PLN);

        budget.ChangeExpenseAmount(expense.Id, newAmount);

        Assert.Equal(newAmount, expense.Amount);
        Assert.Null(expense.ConvertedAmount);
        Assert.Null(expense.ConversionDate);
        Assert.Equal(new Money(300m, Currency.PLN), budget.TotalExpenses);
        Assert.Equal(new Money(-300m, Currency.PLN), budget.ActualFinancialResult);
    }

    [Fact]
    public void ChangeExpenseAmount_RaisesExpenseAmountChangedEvent()
    {
        var budget = CreateBudget();
        var expense = budget.AddExpense(
            new ExpenseId(Guid.NewGuid()),
            CreateExpenseCategory(budget.OwnerId),
            "Hotel",
            new Money(100m, Currency.EUR),
            new DateOnly(2026, 7, 12),
            new Money(425m, Currency.PLN),
            new DateOnly(2026, 7, 12));
        var newAmount = new Money(120m, Currency.EUR);
        var newConvertedAmount = new Money(510m, Currency.PLN);
        var newConversionDate = new DateOnly(2026, 7, 13);
        budget.ClearDomainEvents();
        var before = DateTimeOffset.UtcNow;

        budget.ChangeExpenseAmount(expense.Id, newAmount, newConvertedAmount, newConversionDate);

        var after = DateTimeOffset.UtcNow;
        var domainEvent = Assert.IsType<ExpenseAmountChangedEvent>(Assert.Single(budget.DomainEvents));
        Assert.Equal(budget.Id, domainEvent.BudgetId);
        Assert.Equal(expense.Id, domainEvent.ExpenseId);
        Assert.Equal(expense.CategoryId, domainEvent.CategoryId);
        Assert.Equal(new Money(100m, Currency.EUR), domainEvent.PreviousAmount);
        Assert.Equal(newAmount, domainEvent.NewAmount);
        Assert.Equal(new Money(425m, Currency.PLN), domainEvent.PreviousConvertedAmount);
        Assert.Equal(newConvertedAmount, domainEvent.NewConvertedAmount);
        Assert.Equal(new DateOnly(2026, 7, 12), domainEvent.PreviousConversionDate);
        Assert.Equal(newConversionDate, domainEvent.NewConversionDate);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void ChangeSavingAmount_ChangesAmountAndRecalculatesTotal()
    {
        var budget = CreateBudget();
        var saving = budget.AddSaving(
            new SavingId(Guid.NewGuid()),
            CreateSavingCategory(budget.OwnerId),
            "Emergency fund",
            new Money(1000m, Currency.PLN),
            new DateOnly(2026, 7, 15));
        var newAmount = new Money(1250m, Currency.PLN);

        budget.ChangeSavingAmount(saving.Id, newAmount);

        Assert.Equal(newAmount, saving.Amount);
        Assert.Null(saving.ConvertedAmount);
        Assert.Null(saving.ConversionDate);
        Assert.Equal(new Money(1250m, Currency.PLN), budget.TotalSavings);
        Assert.Equal(new Money(-1250m, Currency.PLN), budget.ActualFinancialResult);
    }

    [Fact]
    public void ActualFinancialResult_SubtractsExpensesAndSavingsFromIncome()
    {
        var budget = CreateBudget();

        budget.AddIncome(
            new IncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budget.OwnerId),
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10));
        budget.AddExpense(
            new ExpenseId(Guid.NewGuid()),
            CreateExpenseCategory(budget.OwnerId),
            "Rent",
            new Money(2000m, Currency.PLN),
            new DateOnly(2026, 7, 2));
        budget.AddSaving(
            new SavingId(Guid.NewGuid()),
            CreateSavingCategory(budget.OwnerId),
            "Emergency fund",
            new Money(1000m, Currency.PLN),
            new DateOnly(2026, 7, 15));

        Assert.Equal(new Money(2000m, Currency.PLN), budget.ActualFinancialResult);
    }

    [Fact]
    public void ChangeSavingAmount_RaisesSavingAmountChangedEvent()
    {
        var budget = CreateBudget();
        var saving = budget.AddSaving(
            new SavingId(Guid.NewGuid()),
            CreateSavingCategory(budget.OwnerId),
            "Broker transfer",
            new Money(500m, Currency.EUR),
            new DateOnly(2026, 7, 16),
            new Money(2125m, Currency.PLN),
            new DateOnly(2026, 7, 16));
        var newAmount = new Money(600m, Currency.EUR);
        var newConvertedAmount = new Money(2550m, Currency.PLN);
        var newConversionDate = new DateOnly(2026, 7, 17);
        budget.ClearDomainEvents();
        var before = DateTimeOffset.UtcNow;

        budget.ChangeSavingAmount(saving.Id, newAmount, newConvertedAmount, newConversionDate);

        var after = DateTimeOffset.UtcNow;
        var domainEvent = Assert.IsType<SavingAmountChangedEvent>(Assert.Single(budget.DomainEvents));
        Assert.Equal(budget.Id, domainEvent.BudgetId);
        Assert.Equal(saving.Id, domainEvent.SavingId);
        Assert.Equal(saving.CategoryId, domainEvent.CategoryId);
        Assert.Equal(new Money(500m, Currency.EUR), domainEvent.PreviousAmount);
        Assert.Equal(newAmount, domainEvent.NewAmount);
        Assert.Equal(new Money(2125m, Currency.PLN), domainEvent.PreviousConvertedAmount);
        Assert.Equal(newConvertedAmount, domainEvent.NewConvertedAmount);
        Assert.Equal(new DateOnly(2026, 7, 16), domainEvent.PreviousConversionDate);
        Assert.Equal(newConversionDate, domainEvent.NewConversionDate);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void ChangeIncomeCategory_ChangesCategoryAndRaisesIncomeCategoryChangedEvent()
    {
        var budget = CreateBudget();
        var income = budget.AddIncome(
            new IncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budget.OwnerId),
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10));
        var previousCategoryId = income.CategoryId;
        var newCategory = CreateIncomeCategory(budget.OwnerId);
        budget.ClearDomainEvents();
        var before = DateTimeOffset.UtcNow;

        budget.ChangeIncomeCategory(income.Id, newCategory);

        var after = DateTimeOffset.UtcNow;
        Assert.Equal(newCategory.Id, income.CategoryId);
        var domainEvent = Assert.IsType<IncomeCategoryChangedEvent>(Assert.Single(budget.DomainEvents));
        Assert.Equal(budget.Id, domainEvent.BudgetId);
        Assert.Equal(income.Id, domainEvent.IncomeId);
        Assert.Equal(previousCategoryId, domainEvent.PreviousCategoryId);
        Assert.Equal(newCategory.Id, domainEvent.NewCategoryId);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void ChangeExpenseTitle_ChangesTitleAndRaisesExpenseTitleChangedEvent()
    {
        var budget = CreateBudget();
        var expense = budget.AddExpense(
            new ExpenseId(Guid.NewGuid()),
            CreateExpenseCategory(budget.OwnerId),
            "Groceries",
            new Money(250m, Currency.PLN),
            new DateOnly(2026, 7, 12));
        budget.ClearDomainEvents();
        var before = DateTimeOffset.UtcNow;

        budget.ChangeExpenseTitle(expense.Id, " Food ");

        var after = DateTimeOffset.UtcNow;
        Assert.Equal("Food", expense.Title);
        var domainEvent = Assert.IsType<ExpenseTitleChangedEvent>(Assert.Single(budget.DomainEvents));
        Assert.Equal(budget.Id, domainEvent.BudgetId);
        Assert.Equal(expense.Id, domainEvent.ExpenseId);
        Assert.Equal(expense.CategoryId, domainEvent.CategoryId);
        Assert.Equal("Groceries", domainEvent.PreviousTitle);
        Assert.Equal("Food", domainEvent.NewTitle);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void ChangeSavingOccurredDate_ChangesDateAndRaisesSavingOccurredDateChangedEvent()
    {
        var budget = CreateBudget();
        var saving = budget.AddSaving(
            new SavingId(Guid.NewGuid()),
            CreateSavingCategory(budget.OwnerId),
            "Emergency fund",
            new Money(1000m, Currency.PLN),
            new DateOnly(2026, 7, 15));
        var newOccurredDate = new DateOnly(2026, 7, 20);
        budget.ClearDomainEvents();
        var before = DateTimeOffset.UtcNow;

        budget.ChangeSavingOccurredDate(saving.Id, newOccurredDate);

        var after = DateTimeOffset.UtcNow;
        Assert.Equal(newOccurredDate, saving.OccurredDate);
        var domainEvent = Assert.IsType<SavingOccurredDateChangedEvent>(Assert.Single(budget.DomainEvents));
        Assert.Equal(budget.Id, domainEvent.BudgetId);
        Assert.Equal(saving.Id, domainEvent.SavingId);
        Assert.Equal(saving.CategoryId, domainEvent.CategoryId);
        Assert.Equal(new DateOnly(2026, 7, 15), domainEvent.PreviousOccurredDate);
        Assert.Equal(newOccurredDate, domainEvent.NewOccurredDate);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void RemoveExpense_MarksExpenseAsRemovedAndRecalculatesTotals()
    {
        var budget = CreateBudget();
        budget.AddIncome(
            new IncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budget.OwnerId),
            "Salary",
            new Money(1000m, Currency.PLN),
            new DateOnly(2026, 7, 10));
        var expense = budget.AddExpense(
            new ExpenseId(Guid.NewGuid()),
            CreateExpenseCategory(budget.OwnerId),
            "Groceries",
            new Money(250m, Currency.PLN),
            new DateOnly(2026, 7, 12));
        budget.AddSaving(
            new SavingId(Guid.NewGuid()),
            CreateSavingCategory(budget.OwnerId),
            "Emergency fund",
            new Money(100m, Currency.PLN),
            new DateOnly(2026, 7, 15));
        var before = DateTimeOffset.UtcNow;

        budget.RemoveExpense(expense.Id, " Duplicate entry ");

        var after = DateTimeOffset.UtcNow;
        Assert.Equal(BudgetEntryStatus.Removed, expense.Status);
        Assert.True(expense.IsRemoved);
        Assert.Equal("Duplicate entry", expense.RemovalReason);
        Assert.NotNull(expense.RemovedOnUtc);
        Assert.InRange(expense.RemovedOnUtc.Value, before, after);
        Assert.Contains(expense, budget.Expenses);
        Assert.Equal(Money.Zero(Currency.PLN), budget.TotalExpenses);
        Assert.Equal(new Money(900m, Currency.PLN), budget.ActualFinancialResult);
    }

    [Fact]
    public void RemoveIncome_RaisesIncomeRemovedEvent()
    {
        var budget = CreateBudget();
        var income = budget.AddIncome(
            new IncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budget.OwnerId),
            "Salary",
            new Money(1000m, Currency.EUR),
            new DateOnly(2026, 7, 10),
            new Money(4250m, Currency.PLN),
            new DateOnly(2026, 7, 9));
        budget.ClearDomainEvents();
        var before = DateTimeOffset.UtcNow;

        budget.RemoveIncome(income.Id, "Wrong account");

        var after = DateTimeOffset.UtcNow;
        var domainEvent = Assert.IsType<IncomeRemovedEvent>(Assert.Single(budget.DomainEvents));
        Assert.Equal(budget.Id, domainEvent.BudgetId);
        Assert.Equal(income.Id, domainEvent.IncomeId);
        Assert.Equal(income.CategoryId, domainEvent.CategoryId);
        Assert.Equal(income.Title, domainEvent.Title);
        Assert.Equal(income.Amount, domainEvent.Amount);
        Assert.Equal(income.ConvertedAmount, domainEvent.ConvertedAmount);
        Assert.Equal(income.ConversionDate, domainEvent.ConversionDate);
        Assert.Equal(income.OccurredDate, domainEvent.OccurredDate);
        Assert.Equal("Wrong account", domainEvent.RemovalReason);
        Assert.InRange(domainEvent.RemovedOnUtc, before, after);
    }

    [Fact]
    public void RemoveSaving_RaisesSavingRemovedEventAndRecalculatesTotals()
    {
        var budget = CreateBudget();
        var saving = budget.AddSaving(
            new SavingId(Guid.NewGuid()),
            CreateSavingCategory(budget.OwnerId),
            "Emergency fund",
            new Money(1000m, Currency.PLN),
            new DateOnly(2026, 7, 15));
        budget.ClearDomainEvents();
        var before = DateTimeOffset.UtcNow;

        budget.RemoveSaving(saving.Id, "Duplicated transfer");

        var after = DateTimeOffset.UtcNow;
        Assert.Equal(Money.Zero(Currency.PLN), budget.TotalSavings);
        Assert.Equal(Money.Zero(Currency.PLN), budget.ActualFinancialResult);
        var domainEvent = Assert.IsType<SavingRemovedEvent>(Assert.Single(budget.DomainEvents));
        Assert.Equal(budget.Id, domainEvent.BudgetId);
        Assert.Equal(saving.Id, domainEvent.SavingId);
        Assert.Equal(saving.CategoryId, domainEvent.CategoryId);
        Assert.Equal("Duplicated transfer", domainEvent.RemovalReason);
        Assert.InRange(domainEvent.RemovedOnUtc, before, after);
    }

    [Theory]
    [InlineData(BudgetCategoryType.Expense)]
    [InlineData(BudgetCategoryType.Saving)]
    public void AddIncome_Throws_WhenCategoryIsNotIncomeCategory(BudgetCategoryType categoryType)
    {
        var budget = CreateBudget();

        Assert.Throws<ArgumentException>(() => budget.AddIncome(
            new IncomeId(Guid.NewGuid()),
            CreateCategory(budget.OwnerId, categoryType),
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10)));
    }

    [Theory]
    [InlineData(BudgetCategoryType.Income)]
    [InlineData(BudgetCategoryType.Saving)]
    public void AddExpense_Throws_WhenCategoryIsNotExpenseCategory(BudgetCategoryType categoryType)
    {
        var budget = CreateBudget();

        Assert.Throws<ArgumentException>(() => budget.AddExpense(
            new ExpenseId(Guid.NewGuid()),
            CreateCategory(budget.OwnerId, categoryType),
            "Groceries",
            new Money(250m, Currency.PLN),
            new DateOnly(2026, 7, 10)));
    }

    [Theory]
    [InlineData(BudgetCategoryType.Income)]
    [InlineData(BudgetCategoryType.Expense)]
    public void AddSaving_Throws_WhenCategoryIsNotSavingCategory(BudgetCategoryType categoryType)
    {
        var budget = CreateBudget();

        Assert.Throws<ArgumentException>(() => budget.AddSaving(
            new SavingId(Guid.NewGuid()),
            CreateCategory(budget.OwnerId, categoryType),
            "Emergency fund",
            new Money(1000m, Currency.PLN),
            new DateOnly(2026, 7, 10)));
    }

    [Fact]
    public void AddExpense_Throws_WhenCategoryBelongsToDifferentOwner()
    {
        var budget = CreateBudget();
        var category = CreateExpenseCategory(new OwnerId(Guid.NewGuid()));

        Assert.Throws<InvalidOperationException>(() => budget.AddExpense(
            new ExpenseId(Guid.NewGuid()),
            category,
            "Groceries",
            new Money(250m, Currency.PLN),
            new DateOnly(2026, 7, 10)));
    }

    [Fact]
    public void AddExpense_Throws_WhenCategoryIsArchived()
    {
        var budget = CreateBudget();
        var category = CreateExpenseCategory(budget.OwnerId);
        category.Archive();

        Assert.Throws<InvalidOperationException>(() => budget.AddExpense(
            new ExpenseId(Guid.NewGuid()),
            category,
            "Groceries",
            new Money(250m, Currency.PLN),
            new DateOnly(2026, 7, 10)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddExpense_Throws_WhenTitleIsEmpty(string? title)
    {
        var budget = CreateBudget();

        Assert.Throws<ArgumentException>(() => budget.AddExpense(
            new ExpenseId(Guid.NewGuid()),
            CreateExpenseCategory(budget.OwnerId),
            title!,
            new Money(250m, Currency.PLN),
            new DateOnly(2026, 7, 10)));
    }

    [Fact]
    public void AddIncome_Throws_WhenTitleIsTooLong()
    {
        var budget = CreateBudget();

        Assert.Throws<ArgumentException>(() => budget.AddIncome(
            new IncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budget.OwnerId),
            new string('a', 101),
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10)));
    }

    [Theory]
    [InlineData(2026, 6, 30)]
    [InlineData(2026, 8, 1)]
    public void AddExpense_Throws_WhenDateIsOutsideBudgetPeriod(int year, int month, int day)
    {
        var budget = CreateBudget();

        Assert.Throws<ArgumentOutOfRangeException>(() => budget.AddExpense(
            new ExpenseId(Guid.NewGuid()),
            CreateExpenseCategory(budget.OwnerId),
            "Groceries",
            new Money(250m, Currency.PLN),
            new DateOnly(year, month, day)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddIncome_Throws_WhenAmountIsNotPositive(decimal amount)
    {
        var budget = CreateBudget();

        Assert.Throws<ArgumentOutOfRangeException>(() => budget.AddIncome(
            new IncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budget.OwnerId),
            "Salary",
            new Money(amount, Currency.PLN),
            new DateOnly(2026, 7, 10)));
    }

    [Fact]
    public void AddIncome_Throws_WhenConvertedAmountIsProvidedForDefaultCurrency()
    {
        var budget = CreateBudget();

        Assert.Throws<ArgumentException>(() => budget.AddIncome(
            new IncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budget.OwnerId),
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10),
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10)));
    }

    [Fact]
    public void AddExpense_Throws_WhenConvertedAmountIsMissingForForeignCurrency()
    {
        var budget = CreateBudget();

        Assert.Throws<ArgumentException>(() => budget.AddExpense(
            new ExpenseId(Guid.NewGuid()),
            CreateExpenseCategory(budget.OwnerId),
            "Hotel",
            new Money(100m, Currency.EUR),
            new DateOnly(2026, 7, 10)));
    }

    [Fact]
    public void AddExpense_Throws_WhenConvertedAmountDoesNotUseDefaultCurrency()
    {
        var budget = CreateBudget();

        Assert.Throws<ArgumentException>(() => budget.AddExpense(
            new ExpenseId(Guid.NewGuid()),
            CreateExpenseCategory(budget.OwnerId),
            "Hotel",
            new Money(100m, Currency.EUR),
            new DateOnly(2026, 7, 10),
            new Money(110m, Currency.USD),
            new DateOnly(2026, 7, 10)));
    }

    [Fact]
    public void AddIncome_Throws_WhenIdAlreadyExists()
    {
        var budget = CreateBudget();
        var incomeId = new IncomeId(Guid.NewGuid());

        budget.AddIncome(
            incomeId,
            CreateIncomeCategory(budget.OwnerId),
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10));

        Assert.Throws<InvalidOperationException>(() => budget.AddIncome(
            incomeId,
            CreateIncomeCategory(budget.OwnerId),
            "Bonus",
            new Money(1000m, Currency.PLN),
            new DateOnly(2026, 7, 11)));
    }

    [Fact]
    public void AddExpense_Throws_WhenBudgetIsClosed()
    {
        var budget = CreateBudget();
        budget.Close();

        Assert.Throws<InvalidOperationException>(() => budget.AddExpense(
            new ExpenseId(Guid.NewGuid()),
            CreateExpenseCategory(budget.OwnerId),
            "Groceries",
            new Money(250m, Currency.PLN),
            new DateOnly(2026, 7, 10)));
    }

    [Fact]
    public void ChangeExpenseAmount_Throws_WhenBudgetIsClosed()
    {
        var budget = CreateBudget();
        var expense = budget.AddExpense(
            new ExpenseId(Guid.NewGuid()),
            CreateExpenseCategory(budget.OwnerId),
            "Groceries",
            new Money(250m, Currency.PLN),
            new DateOnly(2026, 7, 10));
        budget.Close();

        Assert.Throws<InvalidOperationException>(() => budget.ChangeExpenseAmount(
            expense.Id,
            new Money(300m, Currency.PLN)));
    }

    [Fact]
    public void ChangeIncomeAmount_Throws_WhenConvertedAmountIsMissingForForeignCurrency()
    {
        var budget = CreateBudget();
        var income = budget.AddIncome(
            new IncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budget.OwnerId),
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10));

        Assert.Throws<ArgumentException>(() => budget.ChangeIncomeAmount(
            income.Id,
            new Money(1000m, Currency.EUR)));
    }

    [Fact]
    public void ChangeSavingAmount_Throws_WhenSavingDoesNotExist()
    {
        var budget = CreateBudget();

        Assert.Throws<InvalidOperationException>(() => budget.ChangeSavingAmount(
            new SavingId(Guid.NewGuid()),
            new Money(1000m, Currency.PLN)));
    }

    [Fact]
    public void ChangeExpenseCategory_Throws_WhenCategoryIsNotExpenseCategory()
    {
        var budget = CreateBudget();
        var expense = budget.AddExpense(
            new ExpenseId(Guid.NewGuid()),
            CreateExpenseCategory(budget.OwnerId),
            "Groceries",
            new Money(250m, Currency.PLN),
            new DateOnly(2026, 7, 10));

        Assert.Throws<ArgumentException>(() => budget.ChangeExpenseCategory(
            expense.Id,
            CreateIncomeCategory(budget.OwnerId)));
    }

    [Fact]
    public void ChangeIncomeTitle_Throws_WhenTitleIsEmpty()
    {
        var budget = CreateBudget();
        var income = budget.AddIncome(
            new IncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budget.OwnerId),
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10));

        Assert.Throws<ArgumentException>(() => budget.ChangeIncomeTitle(income.Id, " "));
    }

    [Fact]
    public void ChangeSavingOccurredDate_Throws_WhenDateIsOutsideBudgetPeriod()
    {
        var budget = CreateBudget();
        var saving = budget.AddSaving(
            new SavingId(Guid.NewGuid()),
            CreateSavingCategory(budget.OwnerId),
            "Emergency fund",
            new Money(1000m, Currency.PLN),
            new DateOnly(2026, 7, 10));

        Assert.Throws<ArgumentOutOfRangeException>(() => budget.ChangeSavingOccurredDate(
            saving.Id,
            new DateOnly(2026, 8, 1)));
    }

    [Fact]
    public void RemoveExpense_Throws_WhenRemovalReasonIsEmpty()
    {
        var budget = CreateBudget();
        var expense = budget.AddExpense(
            new ExpenseId(Guid.NewGuid()),
            CreateExpenseCategory(budget.OwnerId),
            "Groceries",
            new Money(250m, Currency.PLN),
            new DateOnly(2026, 7, 10));

        Assert.Throws<ArgumentException>(() => budget.RemoveExpense(expense.Id, " "));
    }

    [Fact]
    public void RemoveSaving_Throws_WhenRemovalReasonIsTooLong()
    {
        var budget = CreateBudget();
        var saving = budget.AddSaving(
            new SavingId(Guid.NewGuid()),
            CreateSavingCategory(budget.OwnerId),
            "Emergency fund",
            new Money(1000m, Currency.PLN),
            new DateOnly(2026, 7, 10));

        Assert.Throws<ArgumentException>(() => budget.RemoveSaving(saving.Id, new string('a', 301)));
    }

    [Fact]
    public void RemoveIncome_Throws_WhenIncomeIsAlreadyRemoved()
    {
        var budget = CreateBudget();
        var income = budget.AddIncome(
            new IncomeId(Guid.NewGuid()),
            CreateIncomeCategory(budget.OwnerId),
            "Salary",
            new Money(5000m, Currency.PLN),
            new DateOnly(2026, 7, 10));
        budget.RemoveIncome(income.Id, "Wrong account");

        Assert.Throws<InvalidOperationException>(() => budget.RemoveIncome(income.Id, "Duplicate"));
    }

    [Fact]
    public void ChangeExpenseAmount_Throws_WhenExpenseIsRemoved()
    {
        var budget = CreateBudget();
        var expense = budget.AddExpense(
            new ExpenseId(Guid.NewGuid()),
            CreateExpenseCategory(budget.OwnerId),
            "Groceries",
            new Money(250m, Currency.PLN),
            new DateOnly(2026, 7, 10));
        budget.RemoveExpense(expense.Id, "Duplicate entry");

        Assert.Throws<InvalidOperationException>(() => budget.ChangeExpenseAmount(
            expense.Id,
            new Money(300m, Currency.PLN)));
    }

    [Fact]
    public void RemoveSaving_Throws_WhenBudgetIsClosed()
    {
        var budget = CreateBudget();
        var saving = budget.AddSaving(
            new SavingId(Guid.NewGuid()),
            CreateSavingCategory(budget.OwnerId),
            "Emergency fund",
            new Money(1000m, Currency.PLN),
            new DateOnly(2026, 7, 10));
        budget.Close();

        Assert.Throws<InvalidOperationException>(() => budget.RemoveSaving(saving.Id, "Duplicate"));
    }

    [Fact]
    public void AddSaving_Throws_WhenIdAlreadyExists()
    {
        var budget = CreateBudget();
        var savingId = new SavingId(Guid.NewGuid());

        budget.AddSaving(
            savingId,
            CreateSavingCategory(budget.OwnerId),
            "Emergency fund",
            new Money(1000m, Currency.PLN),
            new DateOnly(2026, 7, 10));

        Assert.Throws<InvalidOperationException>(() => budget.AddSaving(
            savingId,
            CreateSavingCategory(budget.OwnerId),
            "Broker transfer",
            new Money(500m, Currency.PLN),
            new DateOnly(2026, 7, 11)));
    }

    [Fact]
    public void Close_RaisesBudgetStatusChangedEvent()
    {
        var budget = CreateBudget();
        var before = DateTimeOffset.UtcNow;

        budget.Close();

        var after = DateTimeOffset.UtcNow;
        var domainEvent = Assert.IsType<BudgetStatusChangedEvent>(Assert.Single(budget.DomainEvents));
        Assert.Equal(budget.Id, domainEvent.BudgetId);
        Assert.Equal(BudgetStatus.Active, domainEvent.PreviousStatus);
        Assert.Equal(BudgetStatus.Closed, domainEvent.NewStatus);
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    private static Budget CreateBudget()
        => new(
            new BudgetId(Guid.NewGuid()),
            new OwnerId(Guid.NewGuid()),
            new BudgetPeriod(2026, 7),
            Currency.PLN);

    private static BudgetCategory CreateIncomeCategory(OwnerId ownerId)
        => CreateCategory(ownerId, BudgetCategoryType.Income);

    private static BudgetCategory CreateExpenseCategory(OwnerId ownerId)
        => CreateCategory(ownerId, BudgetCategoryType.Expense);

    private static BudgetCategory CreateSavingCategory(OwnerId ownerId)
        => CreateCategory(ownerId, BudgetCategoryType.Saving);

    private static BudgetCategory CreateCategory(OwnerId ownerId, BudgetCategoryType type)
        => new(
            new BudgetCategoryId(Guid.NewGuid()),
            ownerId,
            $"{type} category",
            type);
}
