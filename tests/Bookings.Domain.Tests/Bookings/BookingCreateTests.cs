using Bookings.Domain.Abstractions;
using Bookings.Domain.Bookings;
using Bookings.Domain.Bookings.Events;
using Microsoft.Extensions.Time.Testing;
using static Bookings.Domain.Tests.TestData.BookingMother;

namespace Bookings.Domain.Tests.Bookings
{
    public sealed class BookingCreateTests
    {
        private readonly FakeTimeProvider _clock = new(Now);

        [Fact]
        public void Create_WithFutureStart_IsPendingAndRaisesBookingCreated()
        {
            var booking = Pending(_clock);

            Assert.Equal(BookingStatus.Pending, booking.Status);
            Assert.Equal(Now, booking.CreatedAt);
            Assert.Null(booking.ConfirmedAt);
            Assert.Null(booking.CancelledAt);

            var created = Assert.IsType<BookingCreated>(Assert.Single(booking.DomainEvents));
            Assert.Equal(booking.Id, created.BookingId);
        }

        [Theory]
        [InlineData(0)]    // starts exactly now
        [InlineData(-60)]  // started an hour ago
        public void Create_WithStartNotInFuture_ThrowsStartInPast(int startOffsetInMinutes)
        {
            var start = Now.AddMinutes(startOffsetInMinutes);
            var range = DateRange.Create(start, start.AddHours(1));

            var exception = Assert.Throws<DomainException>(() => Booking.Create(
                Customer.Create("Ana", "ana@example.com"),
                range,
                ResourceId.Create("room-A"),
                _clock));

            Assert.Equal(BookingErrors.StartInPast.Code, exception.Code);
        }
    }
}
