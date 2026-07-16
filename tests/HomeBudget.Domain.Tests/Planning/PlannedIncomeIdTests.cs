using HomeBudget.Domain.Planning;

namespace HomeBudget.Domain.Tests.Planning;

public sealed class PlannedIncomeIdTests
{
    [Fact]
    public void Constructor_SetsValue()
    {
        var value = Guid.NewGuid();

        var id = new PlannedIncomeId(value);

        Assert.Equal(value, id.Value);
    }

    [Fact]
    public void Constructor_Throws_WhenValueIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new PlannedIncomeId(Guid.Empty));
    }
}
