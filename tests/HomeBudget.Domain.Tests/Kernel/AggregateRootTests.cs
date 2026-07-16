using HomeBudget.Domain.Kernel;

namespace HomeBudget.Domain.Tests.Kernel;

public sealed class AggregateRootTests
{
    [Fact]
    public void RaiseDomainEvent_AddsEventToAggregate()
    {
        var aggregate = new TestAggregate(TestId.New());
        var occurredOnUtc = DateTimeOffset.UtcNow;

        aggregate.DoSomething(occurredOnUtc);

        var domainEvent = Assert.Single(aggregate.DomainEvents);
        Assert.Equal(occurredOnUtc, domainEvent.OccurredOnUtc);
    }

    [Fact]
    public void ClearDomainEvents_RemovesRaisedEvents()
    {
        var aggregate = new TestAggregate(TestId.New());
        aggregate.DoSomething(DateTimeOffset.UtcNow);

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void RaiseDomainEvent_Throws_WhenEventIsNull()
    {
        var aggregate = new TestAggregate(TestId.New());

        Assert.Throws<ArgumentNullException>(aggregate.RaiseNullEventForTest);
    }

    private sealed class TestAggregate : AggregateRoot<TestId>
    {
        public TestAggregate(TestId id)
            : base(id)
        {
        }

        public void DoSomething(DateTimeOffset occurredOnUtc)
            => RaiseDomainEvent(new TestDomainEvent(occurredOnUtc));

        public void RaiseNullEventForTest() => RaiseDomainEvent(null!);
    }

    private sealed record TestDomainEvent(DateTimeOffset OccurredOnUtc) : IDomainEvent;

    private readonly record struct TestId(Guid Value) : IStronglyTypedId<Guid>
    {
        public static TestId New() => new(Guid.CreateVersion7());
    }
}
