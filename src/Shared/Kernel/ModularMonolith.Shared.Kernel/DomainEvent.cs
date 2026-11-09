namespace ModularMonolith.Shared.Kernel;

/// <summary>
/// Marker for something that happened in the domain. Events are plain data:
/// they carry no behaviour and never reference infrastructure types.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}

/// <summary>
/// Convenience base record for domain events. The timestamp is captured when
/// the event instance is created.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
