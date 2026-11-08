using Bookings.Domain.Abstractions;

namespace Bookings.Domain.Bookings.Events
{
    public sealed record BookingConfirmed(
        BookingId BookingId,
        ResourceId ResourceId,
        DateRange DateRange,
        Email CustomerEmail,
        DateTimeOffset ConfirmedAt) : IDomainEvent;
}
