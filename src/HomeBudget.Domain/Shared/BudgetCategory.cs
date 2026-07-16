using HomeBudget.Domain.Kernel;

namespace HomeBudget.Domain.Shared;

/// <summary>
/// Represents a user-owned budget category.
/// </summary>
public sealed class BudgetCategory : Entity<BudgetCategoryId>
{
    private const int MaxNameLength = 100;

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

    public OwnerId OwnerId { get; }
    public string Name { get; private set; }
    public BudgetCategoryType Type { get; }
    public bool IsArchived { get; private set; }

    public void Rename(string name)
    {
        Name = NormalizeName(name);
    }

    public void Archive()
    {
        IsArchived = true;
    }

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
