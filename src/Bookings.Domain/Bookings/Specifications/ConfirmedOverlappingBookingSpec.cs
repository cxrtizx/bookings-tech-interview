using Bookings.Domain.Abstractions;
using System.Linq.Expressions;

namespace Bookings.Domain.Bookings.Specifications
{
    public sealed class ConfirmedOverlappingBookingSpec(
        ResourceId resourceId,
        DateRange range,
        BookingId excludeId) : Specification<Booking>
    {
        public override Expression<Func<Booking, bool>> ToExpression() =>
            booking => booking.Status == BookingStatus.Confirmed
            && booking.ResourceId == resourceId
            && booking.Id != excludeId
            && booking.DateRange.Start < range.End
            && booking.DateRange.End > range.Start;
    }
}
