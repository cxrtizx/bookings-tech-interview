using Bookings.Contracts;
using Bookings.Notifications.Worker.Notifications;
using Bookings.Notifications.Worker.Persistence;
using MassTransit;

namespace Bookings.Notifications.Worker.Consumers
{
    public sealed class BookingConfirmedConsumer(
        NotificationRecorder recorder,
        TimeProvider clock,
        ILogger<BookingConfirmedConsumer> logger) : IConsumer<BookingConfirmedV1>
    {
        public async Task Consume(ConsumeContext<BookingConfirmedV1> context)
        {
            var message = context.Message;

            using var scope = logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = message.CorrelationId,
                ["EventId"] = message.EventId,
                ["BookingId"] = message.BookingId
            });

            var notification = new Notification
            {
                EventId = message.EventId,
                EventType = nameof(BookingConfirmedV1),
                BookingId = message.BookingId,
                Recipient = message.CustomerEmail,
                Message = $"Your booking of {message.ResourceId} from {message.Start:u} to {message.End:u} is confirmed.",
                CorrelationId = message.CorrelationId,
                OccurredAtUtc = message.OccurredAtUtc,
                CreatedAtUtc = clock.GetUtcNow()
            };

            if (await recorder.TryRecordAsync(notification, context.CancellationToken))
            {
                logger.LogInformation(
                    "Simulated notification sent to {Recipient} for {EventType} of booking {BookingId}",
                    notification.Recipient,
                    notification.EventType,
                    notification.BookingId);
            }
        }
    }
}
