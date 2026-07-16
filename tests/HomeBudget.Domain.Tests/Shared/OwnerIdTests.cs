using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Tests.Shared;

public sealed class OwnerIdTests
{
    [Fact]
    public void Constructor_SetsValue()
    {
        var value = Guid.NewGuid();

        var ownerId = new OwnerId(value);

        Assert.Equal(value, ownerId.Value);
    }

    [Fact]
    public void Constructor_Throws_WhenValueIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new OwnerId(Guid.Empty));
    }
}
