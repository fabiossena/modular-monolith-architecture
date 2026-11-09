using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Kernel;

namespace ModularMonolith.Shared.Platform.Events;

/// <summary>
/// Dispatches domain events to the handlers registered in the DI container.
/// <para>
/// Inside a modular monolith the event never leaves the process: this is the
/// replacement of the message broker used by the "comprehensive" design. The
/// event type itself lives in the contracts assembly of the module that owns
/// it, so a handler can only react to an event it is allowed to know about.
/// </para>
/// </summary>
public sealed class InProcessDomainEventPublisher(IServiceProvider serviceProvider) : IDomainEventPublisher
{
    public async Task PublishAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            foreach (var handler in ResolveHandlers(domainEvent))
                await handler.HandleAsync(domainEvent, cancellationToken).ConfigureAwait(false);
        }
    }

    private IEnumerable<IDomainEventHandler> ResolveHandlers(IDomainEvent domainEvent)
    {
        var handlers = serviceProvider.GetServices<IDomainEventHandler>();
        var eventType = domainEvent.GetType();

        // A handler registered for the concrete event type, or for the generic
        // IDomainEvent contract.
        var concreteHandlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var genericHandlerType = typeof(IDomainEventHandler<>).MakeGenericType(typeof(IDomainEvent));

        return handlers
            .Where(handler => concreteHandlerType.IsInstanceOfType(handler)
                              || genericHandlerType.IsInstanceOfType(handler))
            .ToList();
    }
}
