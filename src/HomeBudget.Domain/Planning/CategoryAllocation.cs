using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Planning;

public sealed class CategoryAllocation : Entity<CategoryAllocationId>
{
    internal CategoryAllocation(
        CategoryAllocationId id,
        BudgetCategoryId categoryId,
        Money amount,
        CategoryAllocationFlexibility flexibility)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(categoryId);

        CategoryId = categoryId;
        Amount = EnsurePositiveAmount(amount);
        Flexibility = EnsureDefined(flexibility);
    }

    public BudgetCategoryId CategoryId { get; private set; }
    public Money Amount { get; private set; }
    public CategoryAllocationFlexibility Flexibility { get; private set; }
    public decimal ExpenseSharePercentage { get; private set; }
    public decimal IncomeSharePercentage { get; private set; }

    internal void ChangeAmount(Money amount)
    {
        Amount = EnsurePositiveAmount(amount);
    }

    internal void ChangeFlexibility(CategoryAllocationFlexibility flexibility)
    {
        Flexibility = EnsureDefined(flexibility);
    }

    internal void UpdatePercentages(decimal expenseSharePercentage, decimal incomeSharePercentage)
    {
        if (expenseSharePercentage < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(expenseSharePercentage), "Expense share percentage cannot be negative.");
        }

        if (incomeSharePercentage < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(incomeSharePercentage), "Income share percentage cannot be negative.");
        }

        ExpenseSharePercentage = expenseSharePercentage;
        IncomeSharePercentage = incomeSharePercentage;
    }

    private static Money EnsurePositiveAmount(Money amount)
    {
        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Category allocation amount must be greater than zero.");
        }

        return amount;
    }

    private static CategoryAllocationFlexibility EnsureDefined(CategoryAllocationFlexibility flexibility)
    {
        if (!Enum.IsDefined(flexibility))
        {
            throw new ArgumentOutOfRangeException(nameof(flexibility), "Category allocation flexibility is invalid.");
        }

        return flexibility;
    }
}
