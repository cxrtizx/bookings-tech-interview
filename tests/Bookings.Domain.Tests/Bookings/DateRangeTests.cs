using Bookings.Domain.Abstractions;
using Bookings.Domain.Bookings;
using static Bookings.Domain.Tests.TestData.BookingMother;

namespace Bookings.Domain.Tests.Bookings
{
    public sealed class DateRangeTests
    {
        [Fact]
        public void Create_WithStartBeforeEnd_KeepsBoundaries()
        {
            var range = DateRange.Create(At("10:00"), At("11:00"));

            Assert.Equal(At("10:00"), range.Start);
            Assert.Equal(At("11:00"), range.End);
        }

        [Theory]
        [InlineData("10:00", "10:00")]
        [InlineData("11:00", "10:00")]
        public void Create_WithStartNotBeforeEnd_ThrowsInvalidDateRange(string start, string end)
        {
            var exception = Assert.Throws<DomainException>(() => DateRange.Create(At(start), At(end)));

            Assert.Equal(BookingErrors.InvalidDateRange.Code, exception.Code);
        }

        [Fact]
        public void Create_WithOffset_StoresSameInstantInUtc()
        {
            var start = new DateTimeOffset(2027, 1, 10, 11, 0, 0, TimeSpan.FromHours(1));
            var end = new DateTimeOffset(2027, 1, 10, 12, 0, 0, TimeSpan.FromHours(1));

            var range = DateRange.Create(start, end);

            Assert.Equal(TimeSpan.Zero, range.Start.Offset);
            Assert.Equal(At("10:00"), range.Start);
            Assert.Equal(At("11:00"), range.End);
        }

        // Half-open intervals [Start, End): adjacent ranges do not overlap (CA4).
        [Theory]
        [InlineData("10:00", "11:00", "10:30", "11:30", true)]   // partial overlap
        [InlineData("10:00", "11:00", "10:15", "10:45", true)]   // contained
        [InlineData("10:00", "11:00", "09:00", "12:00", true)]   // contains
        [InlineData("10:00", "11:00", "10:00", "11:00", true)]   // identical
        [InlineData("10:00", "11:00", "11:00", "12:00", false)]  // adjacent after
        [InlineData("10:00", "11:00", "09:00", "10:00", false)]  // adjacent before
        [InlineData("10:00", "11:00", "12:00", "13:00", false)]  // disjoint
        public void Overlaps_ReturnsExpectedInBothDirections(
            string firstStart,
            string firstEnd,
            string secondStart,
            string secondEnd,
            bool expected)
        {
            var first = Range(firstStart, firstEnd);
            var second = Range(secondStart, secondEnd);

            Assert.Equal(expected, first.Overlaps(second));
            Assert.Equal(expected, second.Overlaps(first));
        }
    }
}
