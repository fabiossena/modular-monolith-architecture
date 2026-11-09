namespace ModularMonolith.Shared.Kernel;

/// <summary>
/// Publishes domain events to the in-process handlers. Inside a modular
/// monolith this replaces the message broker of the "comprehensive" design:
/// the event never leaves the process.
/// </summary>
public interface IDomainEventPublisher
{
    Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
