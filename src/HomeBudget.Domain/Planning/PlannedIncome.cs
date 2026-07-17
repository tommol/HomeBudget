using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Planning;

/// <summary>
/// Represents an expected income entry in a budget plan.
/// </summary>
public sealed class PlannedIncome : Entity<PlannedIncomeId>
{
    private const int MaxTitleLength = 100;

    private PlannedIncome()
    {
        CategoryId = null!;
        Title = string.Empty;
        Amount = null!;
    }

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

    /// <summary>
    /// Gets the identifier of the income category.
    /// </summary>
    public BudgetCategoryId CategoryId { get; private set; }

    /// <summary>
    /// Gets the title of the planned income.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Gets the original planned income amount.
    /// </summary>
    public Money Amount { get; private set; }

    /// <summary>
    /// Gets the date when the income is expected.
    /// </summary>
    public DateOnly ExpectedDate { get; private set; }

    /// <summary>
    /// Gets the planned income amount converted to the budget plan default currency.
    /// </summary>
    public Money? ConvertedAmount { get; private set; }

    /// <summary>
    /// Gets the date of the currency conversion.
    /// </summary>
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
