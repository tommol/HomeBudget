using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Planning;

public sealed class PlannedIncome : Entity<PlannedIncomeId>
{
    private const int MaxTitleLength = 100;

    internal PlannedIncome(
        PlannedIncomeId id,
        BudgetCategoryId categoryId,
        string title,
        Money amount,
        DateOnly expectedDate,
        Money? convertedAmount,
        DateOnly? conversionDate)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(categoryId);
        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Planned income amount must be greater than zero.");
        }

        if (convertedAmount is null && conversionDate is not null)
        {
            throw new ArgumentException("Conversion date cannot be provided without converted income amount.", nameof(conversionDate));
        }

        if (convertedAmount is not null)
        {
            if (convertedAmount.Amount <= 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(convertedAmount), "Converted income amount must be greater than zero.");
            }

            if (conversionDate is null)
            {
                throw new ArgumentException("Conversion date is required when converted income amount is provided.", nameof(conversionDate));
            }
        }

        CategoryId = categoryId;
        Title = NormalizeTitle(title);
        Amount = amount;
        ExpectedDate = expectedDate;
        ConvertedAmount = convertedAmount;
        ConversionDate = conversionDate;
    }

    public BudgetCategoryId CategoryId { get; private set; }
    public string Title { get; private set; }
    public Money Amount { get; private set; }
    public DateOnly ExpectedDate { get; private set; }
    public Money? ConvertedAmount { get; private set; }
    public DateOnly? ConversionDate { get; private set; }

    private static string NormalizeTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Planned income title is required.", nameof(title));
        }

        title = title.Trim();

        if (title.Length > MaxTitleLength)
        {
            throw new ArgumentException($"Planned income title cannot exceed {MaxTitleLength} characters.", nameof(title));
        }

        return title;
    }
}
