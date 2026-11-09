using Bookings.Application.Bookings.Queries;
using Bookings.Application.Bookings.Queries.GetBookings;
using Bookings.Application.Common;
using Bookings.Domain.Abstractions;
using Bookings.Domain.Bookings;

namespace Bookings.Application.Ports
{
    public interface IBookingQueries
    {
        Task<PagedResult<BookingDto>> SearchAsync(
            Specification<Booking> specification,
            BookingSort sort,
            int page,
            int pageSize,
            CancellationToken cancellationToken);

        Task<BookingDto?> GetByIdAsync(
            BookingId bookingId,
            CancellationToken cancellationToken);
    }
}
