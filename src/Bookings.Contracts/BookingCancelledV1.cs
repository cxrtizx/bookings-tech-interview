namespace Bookings.Contracts
{
    public sealed record BookingCancelledV1(
        Guid EventId,
        DateTimeOffset OccurredAtUtc,
        string CorrelationId,
        Guid BookingId,
        string ResourceId,
        DateTimeOffset Start,
        DateTimeOffset End,
        string CustomerEmail,
        string? Reason);
}
