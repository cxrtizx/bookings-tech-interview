using Mediator;

namespace Bookings.Application.Bookings.Queries.GetBookingById
{
    public sealed record GetBookingByIdQuery(Guid BookingId) : IQuery<BookingDto>;
}
