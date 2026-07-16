using HomeBudget.Domain.Kernel;

namespace HomeBudget.Domain.Shared;

/// <summary>
/// Represents a user-owned budget category.
/// </summary>
public sealed class BudgetCategory : Entity<BudgetCategoryId>
{
    private const int MaxNameLength = 100;

    /// <summary>
    /// Initializes a new instance of the <see cref="BudgetCategory"/> class.
    /// </summary>
    /// <param name="id">The identifier of the budget category.</param>
    /// <param name="ownerId">The identifier of the owner of the category.</param>
    /// <param name="name">The category name.</param>
    /// <param name="type">The budget category type.</param>
    public BudgetCategory(
        BudgetCategoryId id,
        OwnerId ownerId,
        string name,
        BudgetCategoryType type)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(ownerId);

        EnsureDefined(type);

        OwnerId = ownerId;
        Name = NormalizeName(name);
        Type = type;
    }

    /// <summary>
    /// Gets the identifier of the owner of the category.
    /// </summary>
    public OwnerId OwnerId { get; }

    /// <summary>
    /// Gets the category name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the category type.
    /// </summary>
    public BudgetCategoryType Type { get; }

    /// <summary>
    /// Gets a value indicating whether the category is archived.
    /// </summary>
    public bool IsArchived { get; private set; }

    /// <summary>
    /// Renames the budget category.
    /// </summary>
    /// <param name="name">The new category name.</param>
    public void Rename(string name)
    {
        Name = NormalizeName(name);
    }

    /// <summary>
    /// Archives the budget category.
    /// </summary>
    public void Archive()
    {
        IsArchived = true;
    }

    /// <summary>
    /// Restores the budget category from the archive.
    /// </summary>
    public void Restore()
    {
        IsArchived = false;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Budget category name is required.", nameof(name));
        }

        name = name.Trim();

        if (name.Length > MaxNameLength)
        {
            throw new ArgumentException($"Budget category name cannot exceed {MaxNameLength} characters.", nameof(name));
        }

        return name;
    }

    private static void EnsureDefined(BudgetCategoryType type)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), "Budget category type is invalid.");
        }
    }
}
