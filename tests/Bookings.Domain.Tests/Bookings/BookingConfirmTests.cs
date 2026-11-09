using Bookings.Domain.Abstractions;
using Bookings.Domain.Bookings;
using Bookings.Domain.Bookings.Events;
using Microsoft.Extensions.Time.Testing;
using static Bookings.Domain.Tests.TestData.BookingMother;

namespace Bookings.Domain.Tests.Bookings
{
    public sealed class BookingConfirmTests
    {
        private readonly FakeTimeProvider _clock = new(Now);

        [Fact]
        public void Confirm_WhenPendingWithoutOverlap_ConfirmsAndRaisesBookingConfirmed()
        {
            var booking = Pending(_clock);
            booking.ClearDomainEvents();
            _clock.Advance(TimeSpan.FromHours(1));

            booking.Confirm(hasConfirmedOverlap: false, _clock);

            Assert.Equal(BookingStatus.Confirmed, booking.Status);
            Assert.Equal(Now.AddHours(1), booking.ConfirmedAt);

            var confirmed = Assert.IsType<BookingConfirmed>(Assert.Single(booking.DomainEvents));
            Assert.Equal(booking.Id, confirmed.BookingId);
            Assert.Equal(booking.ResourceId, confirmed.ResourceId);
            Assert.Equal(booking.DateRange, confirmed.DateRange);
            Assert.Equal(booking.Customer.Email, confirmed.CustomerEmail);
            Assert.Equal(booking.ConfirmedAt, confirmed.ConfirmedAt);
        }

        // CA3: an overlap does not change the state and no event is raised.
        [Fact]
        public void Confirm_WithConfirmedOverlap_ThrowsOverlapAndKeepsState()
        {
            var booking = Pending(_clock);
            booking.ClearDomainEvents();

            var exception = Assert.Throws<DomainException>(() => booking.Confirm(hasConfirmedOverlap: true, _clock));

            Assert.Equal(BookingErrors.Overlap.Code, exception.Code);
            Assert.Equal(BookingStatus.Pending, booking.Status);
            Assert.Null(booking.ConfirmedAt);
            Assert.Empty(booking.DomainEvents);
        }

        [Theory]
        [InlineData(BookingStatus.Confirmed)]
        [InlineData(BookingStatus.Cancelled)]
        public void Confirm_WhenNotPending_ThrowsInvalidTransition(BookingStatus status)
        {
            var booking = InStatus(status, _clock);
            booking.ClearDomainEvents();

            var exception = Assert.Throws<DomainException>(() => booking.Confirm(hasConfirmedOverlap: false, _clock));

            Assert.Equal(BookingErrors.InvalidTransition.Code, exception.Code);
            Assert.Equal(status, booking.Status);
            Assert.Empty(booking.DomainEvents);
        }

        // The transition is checked first: reporting an overlap for a cancelled booking would be misleading.
        [Fact]
        public void Confirm_WhenCancelledAndOverlapping_ThrowsInvalidTransitionNotOverlap()
        {
            var booking = Cancelled(_clock);

            var exception = Assert.Throws<DomainException>(() => booking.Confirm(hasConfirmedOverlap: true, _clock));

            Assert.Equal(BookingErrors.InvalidTransition.Code, exception.Code);
        }
    }
}
