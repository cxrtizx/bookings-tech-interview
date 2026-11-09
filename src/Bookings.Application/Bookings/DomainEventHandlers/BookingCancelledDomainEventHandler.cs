using Bookings.Application.Common;
using Bookings.Application.Ports;
using Bookings.Contracts;
using Bookings.Domain.Bookings.Events;

namespace Bookings.Application.Bookings.DomainEventHandlers
{
    public sealed class BookingCancelledDomainEventHandler(
        IIntegrationEventPublisher publisher,
        ICorrelationContext correlationContext) : IDomainEventHandler<BookingCancelled>
    {
        public Task HandleAsync(BookingCancelled domainEvent, CancellationToken cancellationToken)
        {
            var integrationEvent = new BookingCancelledV1(
                EventId: Guid.NewGuid(),
                OccurredAtUtc: domainEvent.CancelledAt,
                CorrelationId: correlationContext.CorrelationId,
                BookingId: domainEvent.BookingId.Value,
                ResourceId: domainEvent.ResourceId.Value,
                Start: domainEvent.DateRange.Start,
                End: domainEvent.DateRange.End,
                CustomerEmail: domainEvent.CustomerEmail.Value,
                Reason: domainEvent.CancellationReason);

            return publisher.PublishAsync(integrationEvent, cancellationToken);
        }
    }
}
