namespace ModularMonolith.Shared.Kernel;

/// <summary>
/// Handles a single kind of domain event. Modules register their handlers in
/// the DI container; the in-process publisher resolves them at dispatch time.
/// </summary>
public interface IDomainEventHandler
{
    Task HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Strongly typed variant of <see cref="IDomainEventHandler"/>.
/// </summary>
public interface IDomainEventHandler<in TEvent> : IDomainEventHandler
    where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);

    async Task IDomainEventHandler.HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken) =>
        await HandleAsync((TEvent)domainEvent, cancellationToken);
}
