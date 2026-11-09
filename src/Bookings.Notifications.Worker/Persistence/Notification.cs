namespace Bookings.Notifications.Worker.Persistence
{
    // Simulated notification record. The worker has no domain model of its own:
    // it only stores what was "sent", so a plain entity is enough here.
    public sealed class Notification
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required Guid EventId { get; init; }
        public required string EventType { get; init; }
        public required Guid BookingId { get; init; }
        public required string Recipient { get; init; }
        public required string Message { get; init; }
        public required string CorrelationId { get; init; }
        public required DateTimeOffset OccurredAtUtc { get; init; }
        public required DateTimeOffset CreatedAtUtc { get; init; }
    }
}
