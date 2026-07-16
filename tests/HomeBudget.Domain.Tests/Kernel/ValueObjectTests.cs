using HomeBudget.Domain.Kernel;

namespace HomeBudget.Domain.Tests.Kernel;

public sealed class ValueObjectTests
{
    [Fact]
    public void Equals_ReturnsTrue_WhenTypeAndComponentsMatch()
    {
        var left = new TestValue("Food", 100m);
        var right = new TestValue("Food", 100m);

        Assert.Equal(left, right);
        Assert.True(left == right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenComponentsDiffer()
    {
        var left = new TestValue("Food", 100m);
        var right = new TestValue("Food", 200m);

        Assert.NotEqual(left, right);
        Assert.True(left != right);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenRuntimeTypesDiffer()
    {
        ValueObject left = new TestValue("Food", 100m);
        ValueObject right = new OtherTestValue("Food", 100m);

        Assert.NotEqual(left, right);
        Assert.False(left == right);
    }

    private sealed class TestValue : ValueObject
    {
        private readonly string _name;
        private readonly decimal _amount;

        public TestValue(string name, decimal amount)
        {
            _name = name;
            _amount = amount;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return _name;
            yield return _amount;
        }
    }

    private sealed class OtherTestValue : ValueObject
    {
        private readonly string _name;
        private readonly decimal _amount;

        public OtherTestValue(string name, decimal amount)
        {
            _name = name;
            _amount = amount;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return _name;
            yield return _amount;
        }
    }
}
