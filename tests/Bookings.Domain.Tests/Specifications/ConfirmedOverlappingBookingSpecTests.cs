using Bookings.Domain.Bookings;
using Bookings.Domain.Bookings.Specifications;
using Microsoft.Extensions.Time.Testing;
using static Bookings.Domain.Tests.TestData.BookingMother;

namespace Bookings.Domain.Tests.Specifications
{
    // The spec is evaluated in SQL in production; IsSatisfiedBy evaluates the same expression in memory,
    // so these tests guarantee that the spec and DateRange.Overlaps agree on the edge cases.
    public sealed class ConfirmedOverlappingBookingSpecTests
    {
        private readonly FakeTimeProvider _clock = new(Now);
        private readonly BookingId _bookingBeingConfirmed = BookingId.New();

        [Theory]
        [InlineData("10:30", "11:30", true)]   // partial overlap
        [InlineData("10:15", "10:45", true)]   // contained
        [InlineData("09:00", "12:00", true)]   // contains
        [InlineData("11:00", "12:00", false)]  // adjacent after (CA4)
        [InlineData("09:00", "10:00", false)]  // adjacent before
        [InlineData("12:00", "13:00", false)]  // disjoint
        public void IsSatisfiedBy_ConfirmedBookingInSameResource_MatchesOnlyWhenRangesOverlap(
            string start,
            string end,
            bool expected)
        {
            var confirmed = Confirmed(_clock, start: "10:00", end: "11:00");
            var spec = new ConfirmedOverlappingBookingSpec(ResourceId.Create("room-A"), Range(start, end), _bookingBeingConfirmed);

            Assert.Equal(expected, spec.IsSatisfiedBy(confirmed));
        }

        [Fact]
        public void IsSatisfiedBy_PendingOverlappingBooking_ReturnsFalse()
        {
            var pending = Pending(_clock);
            var spec = new ConfirmedOverlappingBookingSpec(pending.ResourceId, pending.DateRange, _bookingBeingConfirmed);

            Assert.False(spec.IsSatisfiedBy(pending));
        }

        [Fact]
        public void IsSatisfiedBy_CancelledOverlappingBooking_ReturnsFalse()
        {
            var cancelled = Cancelled(_clock);
            var spec = new ConfirmedOverlappingBookingSpec(cancelled.ResourceId, cancelled.DateRange, _bookingBeingConfirmed);

            Assert.False(spec.IsSatisfiedBy(cancelled));
        }

        [Fact]
        public void IsSatisfiedBy_ConfirmedBookingInOtherResource_ReturnsFalse()
        {
            var confirmed = Confirmed(_clock, resourceId: "room-B");
            var spec = new ConfirmedOverlappingBookingSpec(ResourceId.Create("room-A"), confirmed.DateRange, _bookingBeingConfirmed);

            Assert.False(spec.IsSatisfiedBy(confirmed));
        }

        [Fact]
        public void IsSatisfiedBy_TheBookingBeingConfirmedItself_ReturnsFalse()
        {
            var confirmed = Confirmed(_clock);
            var spec = new ConfirmedOverlappingBookingSpec(confirmed.ResourceId, confirmed.DateRange, excludeId: confirmed.Id);

            Assert.False(spec.IsSatisfiedBy(confirmed));
        }
    }
}
