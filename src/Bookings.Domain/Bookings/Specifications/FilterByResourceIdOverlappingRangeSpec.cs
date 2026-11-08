using Bookings.Domain.Abstractions;
using System.Linq.Expressions;

namespace Bookings.Domain.Bookings.Specifications
{
    public sealed class FilterByResourceIdOverlappingRangeSpec(
        ResourceId? resourceId,
        DateTimeOffset? from,
        DateTimeOffset? to) : Specification<Booking>
    {
        private readonly DateTimeOffset? _from = from?.ToUniversalTime();
        private readonly DateTimeOffset? _to = to?.ToUniversalTime();

        public override Expression<Func<Booking, bool>> ToExpression() =>
            booking => (resourceId == null || booking.ResourceId == resourceId)
            && (_from == null || booking.DateRange.End > _from)
            && (_to == null || booking.DateRange.Start < _to);
    }
}
