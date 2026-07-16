using HomeBudget.Domain.Planning;

namespace HomeBudget.Domain.Tests.Planning;

public sealed class SavingContributionIdTests
{
    [Fact]
    public void Constructor_SetsValue()
    {
        var value = Guid.NewGuid();

        var id = new SavingContributionId(value);

        Assert.Equal(value, id.Value);
    }

    [Fact]
    public void Constructor_Throws_WhenValueIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new SavingContributionId(Guid.Empty));
    }
}
