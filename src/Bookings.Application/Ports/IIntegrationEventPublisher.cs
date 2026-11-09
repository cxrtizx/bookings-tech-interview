namespace Bookings.Application.Ports
{
    public interface IIntegrationEventPublisher
    {
        Task PublishAsync<TIntegrationEvent>(
            TIntegrationEvent integrationEvent,
            CancellationToken cancellationToken) where TIntegrationEvent : class;
    }
}
