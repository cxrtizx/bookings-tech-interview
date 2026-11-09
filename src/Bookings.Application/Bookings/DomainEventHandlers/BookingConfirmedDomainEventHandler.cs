using Bookings.Application.Common;
using Bookings.Application.Ports;
using Bookings.Contracts;
using Bookings.Domain.Bookings.Events;

namespace Bookings.Application.Bookings.DomainEventHandlers
{
    public sealed class BookingConfirmedDomainEventHandler(
        IIntegrationEventPublisher publisher,
        ICorrelationContext correlationContext) : IDomainEventHandler<BookingConfirmed>
    {
        public Task HandleAsync(BookingConfirmed domainEvent, CancellationToken cancellationToken)
        {
            var integrationEvent = new BookingConfirmedV1(
                EventId: Guid.NewGuid(),
                OccurredAtUtc: domainEvent.ConfirmedAt,
                CorrelationId: correlationContext.CorrelationId,
                BookingId: domainEvent.BookingId.Value,
                ResourceId: domainEvent.ResourceId.Value,
                Start: domainEvent.DateRange.Start,
                End: domainEvent.DateRange.End,
                CustomerEmail: domainEvent.CustomerEmail.Value);

            return publisher.PublishAsync(integrationEvent, cancellationToken);
        }
    }
}
