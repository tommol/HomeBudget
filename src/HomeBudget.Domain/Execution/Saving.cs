using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents a saving entry in a budget.
/// </summary>
public sealed class Saving : Entity<SavingId>
{
    private const int MaxTitleLength = 100;
    private const int MaxRemovalReasonLength = 300;

    private Saving()
    {
        CategoryId = null!;
        Title = string.Empty;
        Amount = null!;
    }

    internal Saving(
        SavingId id,
        BudgetCategoryId categoryId,
        string title,
        Money amount,
        DateOnly occurredDate,
        Money? convertedAmount,
        DateOnly? conversionDate)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(categoryId);

        CategoryId = categoryId;
        Title = NormalizeTitle(title);
        OccurredDate = occurredDate;
        Status = BudgetEntryStatus.Active;
        Amount = null!;
        ChangeAmount(amount, convertedAmount, conversionDate);
    }

    /// <summary>
    /// Gets the identifier of the saving category.
    /// </summary>
    public BudgetCategoryId CategoryId { get; private set; }

    /// <summary>
    /// Gets the title of the saving.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Gets the original saving amount.
    /// </summary>
    public Money Amount { get; private set; }

    /// <summary>
    /// Gets the date when the saving occurred.
    /// </summary>
    public DateOnly OccurredDate { get; private set; }

    /// <summary>
    /// Gets the saving amount converted to the budget default currency.
    /// </summary>
    public Money? ConvertedAmount { get; private set; }

    /// <summary>
    /// Gets the date of the currency conversion.
    /// </summary>
    public DateOnly? ConversionDate { get; private set; }

    /// <summary>
    /// Gets the status of the saving entry.
    /// </summary>
    public BudgetEntryStatus Status { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the saving entry was removed.
    /// </summary>
    public bool IsRemoved => Status == BudgetEntryStatus.Removed;

    /// <summary>
    /// Gets the reason why the saving entry was removed.
    /// </summary>
    public string? RemovalReason { get; private set; }

    /// <summary>
    /// Gets the date and time when the saving entry was removed in UTC.
    /// </summary>
    public DateTimeOffset? RemovedOnUtc { get; private set; }

    internal void ChangeCategory(BudgetCategoryId categoryId)
    {
        ArgumentNullException.ThrowIfNull(categoryId);

        CategoryId = categoryId;
    }

    internal void ChangeTitle(string title)
    {
        Title = NormalizeTitle(title);
    }

    internal void ChangeOccurredDate(DateOnly occurredDate)
    {
        OccurredDate = occurredDate;
    }

    internal void ChangeAmount(Money amount, Money? convertedAmount, DateOnly? conversionDate)
    {
        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Saving amount must be greater than zero.");
        }

        if (convertedAmount is null && conversionDate is not null)
        {
            throw new ArgumentException("Conversion date cannot be provided without converted saving amount.", nameof(conversionDate));
        }

        if (convertedAmount is not null)
        {
            if (convertedAmount.Amount <= 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(convertedAmount), "Converted saving amount must be greater than zero.");
            }

            if (conversionDate is null)
            {
                throw new ArgumentException("Conversion date is required when converted saving amount is provided.", nameof(conversionDate));
            }
        }

        Amount = amount;
        ConvertedAmount = convertedAmount;
        ConversionDate = conversionDate;
    }

    internal void Remove(string removalReason, DateTimeOffset removedOnUtc)
    {
        if (IsRemoved)
        {
            throw new InvalidOperationException("Saving is already removed.");
        }

        Status = BudgetEntryStatus.Removed;
        RemovalReason = NormalizeRemovalReason(removalReason);
        RemovedOnUtc = removedOnUtc;
    }

    private static string NormalizeTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Saving title is required.", nameof(title));
        }

        title = title.Trim();

        if (title.Length > MaxTitleLength)
        {
            throw new ArgumentException($"Saving title cannot exceed {MaxTitleLength} characters.", nameof(title));
        }

        return title;
    }

    private static string NormalizeRemovalReason(string removalReason)
    {
        if (string.IsNullOrWhiteSpace(removalReason))
        {
            throw new ArgumentException("Saving removal reason is required.", nameof(removalReason));
        }

        removalReason = removalReason.Trim();

        if (removalReason.Length > MaxRemovalReasonLength)
        {
            throw new ArgumentException(
                $"Saving removal reason cannot exceed {MaxRemovalReasonLength} characters.",
                nameof(removalReason));
        }

        return removalReason;
    }
}
