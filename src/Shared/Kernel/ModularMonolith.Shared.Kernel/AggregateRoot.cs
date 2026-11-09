namespace ModularMonolith.Shared.Kernel;

/// <summary>
/// Base class for aggregate roots. Aggregates are the only place where domain
/// events may be raised, because an event must always be raised from within a
/// consistency boundary.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    /// <summary>
    /// Returns the pending events and clears the buffer. Called by the
    /// application layer right after the aggregate has been persisted.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> PullDomainEvents()
    {
        var events = _domainEvents.ToArray();
        _domainEvents.Clear();
        return events;
    }
}
