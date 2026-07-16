using HomeBudget.Domain.Kernel;

namespace HomeBudget.Domain.Tests.Kernel;

public sealed class EntityTests
{
    [Fact]
    public void Equals_ReturnsTrue_WhenRuntimeTypeAndIdMatch()
    {
        var id = TestId.New();

        Entity<TestId> left = new TestEntity(id);
        Entity<TestId> right = new TestEntity(id);

        Assert.Equal(left, right);
        Assert.True(left == right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenRuntimeTypesDiffer()
    {
        var id = TestId.New();

        Entity<TestId> left = new TestEntity(id);
        Entity<TestId> right = new OtherTestEntity(id);

        Assert.NotEqual(left, right);
        Assert.False(left == right);
    }

    [Fact]
    public void Constructor_Throws_WhenIdIsDefault()
    {
        var exception = Assert.Throws<ArgumentException>(() => new TestEntity(default));

        Assert.Equal("id", exception.ParamName);
    }

    private sealed class TestEntity : Entity<TestId>
    {
        public TestEntity(TestId id)
            : base(id)
        {
        }
    }

    private sealed class OtherTestEntity : Entity<TestId>
    {
        public OtherTestEntity(TestId id)
            : base(id)
        {
        }
    }

    private readonly record struct TestId(Guid Value) : IStronglyTypedId<Guid>
    {
        public static TestId New() => new(Guid.CreateVersion7());
    }
}
