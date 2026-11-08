using Bookings.Application.Bookings.Queries.GetBookings;
using Bookings.Application.Common;
using Bookings.Domain.Abstractions;
using Bookings.Domain.Bookings;

namespace Bookings.Application.Ports
{
    public interface IBookingQueries
    {
        Task<PagedResult<BookingSummaryDto>> SearchAsync(
            Specification<Booking> specification,
            BookingSort sort,
            int page,
            int pageSize,
            CancellationToken cancellationToken);
    }
}
