using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Planning;

public sealed class SavingContribution : Entity<SavingContributionId>
{
    internal SavingContribution(
        SavingContributionId id,
        BudgetCategoryId categoryId,
        Money amount)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(categoryId);

        CategoryId = categoryId;
        Amount = EnsurePositiveAmount(amount);
    }

    public BudgetCategoryId CategoryId { get; private set; }
    public Money Amount { get; private set; }

    internal void ChangeAmount(Money amount)
    {
        Amount = EnsurePositiveAmount(amount);
    }

    private static Money EnsurePositiveAmount(Money amount)
    {
        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Saving contribution amount must be greater than zero.");
        }

        return amount;
    }
}
