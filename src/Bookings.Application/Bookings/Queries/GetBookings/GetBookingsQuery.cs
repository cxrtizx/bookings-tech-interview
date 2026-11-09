using Bookings.Application.Common;
using Mediator;

namespace Bookings.Application.Bookings.Queries.GetBookings
{
    public sealed record GetBookingsQuery(
        string? ResourceId,
        DateTimeOffset? From,
        DateTimeOffset? To,
        int Page = Paging.DefaultPage,
        int PageSize = Paging.DefaultPageSize,
        string? Sort = null) : IQuery<PagedResult<BookingDto>>;
}
