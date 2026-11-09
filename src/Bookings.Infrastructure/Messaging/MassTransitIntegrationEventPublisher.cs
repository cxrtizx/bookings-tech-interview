using Bookings.Application.Ports;
using MassTransit;

namespace Bookings.Infrastructure.Messaging
{
    public sealed class MassTransitIntegrationEventPublisher(
        IPublishEndpoint publishEndpoint) : IIntegrationEventPublisher
    {
        public Task PublishAsync<TIntegrationEvent>(
            TIntegrationEvent integrationEvent,
            CancellationToken cancellationToken) where TIntegrationEvent : class
        {
            return publishEndpoint.Publish(integrationEvent, cancellationToken);
        }
    }
}
