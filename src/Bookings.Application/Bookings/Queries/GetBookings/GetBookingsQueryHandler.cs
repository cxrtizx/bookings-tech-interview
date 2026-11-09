using Bookings.Application.Common;
using Bookings.Application.Ports;
using Bookings.Domain.Bookings;
using Bookings.Domain.Bookings.Specifications;
using Mediator;

namespace Bookings.Application.Bookings.Queries.GetBookings
{
    public sealed class GetBookingsQueryHandler(IBookingQueries bookingQueries)
        : IQueryHandler<GetBookingsQuery, PagedResult<BookingDto>>
    {
        public async ValueTask<PagedResult<BookingDto>> Handle(
            GetBookingsQuery query,
            CancellationToken cancellationToken)
        {
            var resourceId = query.ResourceId is null ? null : ResourceId.Create(query.ResourceId);
            var spec = new FilterByResourceIdOverlappingRangeSpec(resourceId, query.From, query.To);
            var sort = BookingSort.TryParse(query.Sort, out var parsedSort) ? parsedSort : BookingSort.Default;

            return await bookingQueries.SearchAsync(
                spec,
                sort,
                query.Page,
                query.PageSize,
                cancellationToken);
        }
    }
}
