using Bookings.Domain.Abstractions;

namespace Bookings.Domain.Bookings.Events
{
    public sealed record BookingCreated(BookingId BookingId) : IDomainEvent;
}
