using Bookings.Application.Common;
using Bookings.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Infrastructure
{
    public sealed class DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        public async Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
        {
            foreach (var domainEvent in domainEvents)
            {
                // Resolves IDomainEventHandler<TheConcreteEventType> at runtime.
                var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());

                foreach (dynamic handler in serviceProvider.GetServices(handlerType).OfType<object>())
                {
                    await handler.HandleAsync((dynamic)domainEvent, cancellationToken);
                }
            }
        }
    }
}
