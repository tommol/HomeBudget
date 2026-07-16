using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Tests.Shared;

public sealed class BudgetCategoryTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var id = new BudgetCategoryId(Guid.NewGuid());
        var ownerId = new OwnerId(Guid.NewGuid());

        var category = new BudgetCategory(id, ownerId, "Groceries", BudgetCategoryType.Expense);

        Assert.Equal(id, category.Id);
        Assert.Equal(ownerId, category.OwnerId);
        Assert.Equal("Groceries", category.Name);
        Assert.Equal(BudgetCategoryType.Expense, category.Type);
        Assert.False(category.IsArchived);
    }

    [Fact]
    public void Constructor_TrimsName()
    {
        var category = CreateCategory(name: " Groceries ");

        Assert.Equal("Groceries", category.Name);
    }

    [Fact]
    public void Constructor_AcceptsSavingType()
    {
        var category = CreateCategory(type: BudgetCategoryType.Saving);

        Assert.Equal(BudgetCategoryType.Saving, category.Type);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_Throws_WhenNameIsInvalid(string name)
    {
        Assert.Throws<ArgumentException>(() => CreateCategory(name: name));
    }

    [Fact]
    public void Constructor_Throws_WhenNameIsTooLong()
    {
        var name = new string('a', 101);

        Assert.Throws<ArgumentException>(() => CreateCategory(name: name));
    }

    [Fact]
    public void Constructor_Throws_WhenTypeIsInvalid()
    {
        var invalidType = (BudgetCategoryType)999;

        Assert.Throws<ArgumentOutOfRangeException>(() => CreateCategory(type: invalidType));
    }

    [Fact]
    public void Constructor_Throws_WhenOwnerIdIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new BudgetCategory(
            new BudgetCategoryId(Guid.NewGuid()),
            null!,
            "Groceries",
            BudgetCategoryType.Expense));
    }

    [Fact]
    public void Constructor_Throws_WhenIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new BudgetCategoryId(Guid.Empty));
    }

    [Fact]
    public void Rename_ChangesName()
    {
        var category = CreateCategory();

        category.Rename("Transport");

        Assert.Equal("Transport", category.Name);
    }

    [Fact]
    public void Archive_MarksCategoryAsArchived()
    {
        var category = CreateCategory();

        category.Archive();

        Assert.True(category.IsArchived);
    }

    [Fact]
    public void Restore_MarksCategoryAsActive()
    {
        var category = CreateCategory();

        category.Archive();
        category.Restore();

        Assert.False(category.IsArchived);
    }

    private static BudgetCategory CreateCategory(
        string name = "Groceries",
        BudgetCategoryType type = BudgetCategoryType.Expense)
        => new(
            new BudgetCategoryId(Guid.NewGuid()),
            new OwnerId(Guid.NewGuid()),
            name,
            type);
}
