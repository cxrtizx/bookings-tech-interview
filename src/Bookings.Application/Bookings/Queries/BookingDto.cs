namespace Bookings.Application.Bookings.Queries
{
    public sealed record BookingDto(
        Guid Id,
        string CustomerName,
        string CustomerEmail,
        DateTimeOffset Start,
        DateTimeOffset End,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ConfirmedAt,
        DateTimeOffset? CancelledAt,
        string? CancellationReason,
        string ResourceId,
        string Status);
}
