namespace HomeBudget.Api.Auth;

internal sealed class CurrentOwnerContext : ICurrentOwner
{
    private Guid? _ownerId;

    public Guid OwnerId => _ownerId
        ?? throw new InvalidOperationException("Current owner was not resolved for this request.");

    public void SetOwnerId(Guid ownerId)
    {
        if (ownerId == Guid.Empty)
        {
            throw new ArgumentException("Owner id cannot be empty.", nameof(ownerId));
        }

        _ownerId = ownerId;
    }
}
