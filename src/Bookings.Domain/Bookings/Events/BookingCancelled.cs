using Bookings.Domain.Abstractions;

namespace Bookings.Domain.Bookings.Events
{
    public sealed record BookingCancelled(
        BookingId BookingId,
        ResourceId ResourceId,
        DateRange DateRange,
        Email CustomerEmail,
        DateTimeOffset CancelledAt,
        string? CancellationReason) : IDomainEvent;
}
