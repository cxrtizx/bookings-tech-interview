using Bookings.Application.Common;
using Mediator;

namespace Bookings.Application.Bookings.Queries.GetBookings
{
    public sealed record GetBookingsQuery(
        string? ResourceId,
        DateTimeOffset? From,
        DateTimeOffset? To,
        int Page = 1,
        int PageSize = 20,
        string? Sort = null) : IQuery<PagedResult<BookingSummaryDto>>;
}
