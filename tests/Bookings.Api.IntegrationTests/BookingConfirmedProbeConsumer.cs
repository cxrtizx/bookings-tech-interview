using Bookings.Contracts;
using MassTransit;

namespace Bookings.Api.IntegrationTests
{
    // Stand-in for Notifications.Worker: subscribes to BookingConfirmedV1 in the in-memory test bus.
    // Consuming (not just observing the send) proves the message is routed and deserializable as the contract.
    public sealed class BookingConfirmedProbeConsumer : IConsumer<BookingConfirmedV1>
    {
        public Task Consume(ConsumeContext<BookingConfirmedV1> context) => Task.CompletedTask;
    }
}
