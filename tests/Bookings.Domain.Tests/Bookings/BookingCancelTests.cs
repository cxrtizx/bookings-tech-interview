using Bookings.Domain.Abstractions;
using Bookings.Domain.Bookings;
using Bookings.Domain.Bookings.Events;
using Microsoft.Extensions.Time.Testing;
using static Bookings.Domain.Tests.TestData.BookingMother;

namespace Bookings.Domain.Tests.Bookings
{
    public sealed class BookingCancelTests
    {
        private readonly FakeTimeProvider _clock = new(Now);

        [Theory]
        [InlineData(BookingStatus.Pending)]
        [InlineData(BookingStatus.Confirmed)]
        public void Cancel_WhenPendingOrConfirmed_CancelsAndRaisesBookingCancelled(BookingStatus status)
        {
            var booking = InStatus(status, _clock);
            booking.ClearDomainEvents();
            _clock.Advance(TimeSpan.FromHours(2));

            booking.Cancel("Customer called", _clock);

            Assert.Equal(BookingStatus.Cancelled, booking.Status);
            Assert.Equal(Now.AddHours(2), booking.CancelledAt);
            Assert.Equal("Customer called", booking.CancellationReason);

            var cancelled = Assert.IsType<BookingCancelled>(Assert.Single(booking.DomainEvents));
            Assert.Equal(booking.Id, cancelled.BookingId);
            Assert.Equal(booking.ResourceId, cancelled.ResourceId);
            Assert.Equal(booking.CancelledAt, cancelled.CancelledAt);
            Assert.Equal("Customer called", cancelled.CancellationReason);
        }

        // CA5: cancelling an already cancelled booking is an invalid transition.
        [Fact]
        public void Cancel_WhenAlreadyCancelled_ThrowsInvalidTransition()
        {
            var booking = Cancelled(_clock);
            var firstCancellation = booking.CancelledAt;
            booking.ClearDomainEvents();
            _clock.Advance(TimeSpan.FromHours(1));

            var exception = Assert.Throws<DomainException>(() => booking.Cancel("Again", _clock));

            Assert.Equal(BookingErrors.InvalidTransition.Code, exception.Code);
            Assert.Equal(firstCancellation, booking.CancelledAt);
            Assert.Empty(booking.DomainEvents);
        }

        [Fact]
        public void Cancel_WithPaddedReason_StoresTrimmedReason()
        {
            var booking = Pending(_clock);

            booking.Cancel("   Room maintenance   ", _clock);

            Assert.Equal("Room maintenance", booking.CancellationReason);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Cancel_WithoutReason_StoresNull(string? reason)
        {
            var booking = Pending(_clock);

            booking.Cancel(reason, _clock);

            Assert.Equal(BookingStatus.Cancelled, booking.Status);
            Assert.Null(booking.CancellationReason);
        }

        [Fact]
        public void Cancel_WithReasonAtMaxLengthAfterTrimming_IsAccepted()
        {
            var booking = Pending(_clock);
            var reason = new string('a', Booking.CancellationReasonMaxLength) + "     ";

            booking.Cancel(reason, _clock);

            Assert.Equal(Booking.CancellationReasonMaxLength, booking.CancellationReason!.Length);
        }

        [Fact]
        public void Cancel_WithReasonOverMaxLength_ThrowsAndKeepsState()
        {
            var booking = Pending(_clock);
            var reason = new string('a', Booking.CancellationReasonMaxLength + 1);

            var exception = Assert.Throws<DomainException>(() => booking.Cancel(reason, _clock));

            Assert.Equal(BookingErrors.CancellationReasonTooLong.Code, exception.Code);
            Assert.Equal(BookingStatus.Pending, booking.Status);
        }
    }
}
