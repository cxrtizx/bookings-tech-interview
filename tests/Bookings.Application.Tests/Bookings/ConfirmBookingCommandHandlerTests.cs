using Bookings.Application.Bookings.Commands.ConfirmBooking;
using Bookings.Application.Ports;
using Bookings.Domain.Abstractions;
using Bookings.Domain.Bookings;
using Bookings.Domain.Bookings.Specifications;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Bookings.Application.Tests.Bookings
{
    // The handler orchestrates: load the booking, ask the repository for a confirmed overlap
    // (spec evaluated in the database in production), let the aggregate decide, save once.
    // Ports are replaced with test doubles; the aggregate is real.
    public sealed class ConfirmBookingCommandHandlerTests
    {
        private static readonly DateTimeOffset Now = new(2027, 1, 1, 9, 0, 0, TimeSpan.Zero);

        private readonly FakeTimeProvider _clock = new(Now);
        private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly ConfirmBookingCommandHandler _handler;

        public ConfirmBookingCommandHandlerTests()
        {
            _handler = new ConfirmBookingCommandHandler(_bookingRepository, _unitOfWork, _clock);
        }

        [Fact]
        public async Task Handle_WithoutConfirmedOverlap_ConfirmsBookingAndSavesOnce()
        {
            var booking = GivenPendingBooking();
            GivenConfirmedOverlap(false);

            await _handler.Handle(new ConfirmBookingCommand(booking.Id.Value), CancellationToken.None);

            Assert.Equal(BookingStatus.Confirmed, booking.Status);
            await _bookingRepository.Received(1).AnyAsync(
                Arg.Is<Specification<Booking>>(spec => spec is ConfirmedOverlappingBookingSpec),
                Arg.Any<CancellationToken>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WithConfirmedOverlap_ThrowsOverlapAndDoesNotSave()
        {
            var booking = GivenPendingBooking();
            GivenConfirmedOverlap(true);

            var exception = await Assert.ThrowsAsync<DomainException>(async () =>
                await _handler.Handle(new ConfirmBookingCommand(booking.Id.Value), CancellationToken.None));

            Assert.Equal(BookingErrors.Overlap.Code, exception.Code);
            Assert.Equal(BookingStatus.Pending, booking.Status);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        private Booking GivenPendingBooking()
        {
            var booking = Booking.Create(
                Customer.Create("Ana", "ana@example.com"),
                DateRange.Create(Now.AddDays(9), Now.AddDays(9).AddHours(1)),
                ResourceId.Create("room-A"),
                _clock);

            _bookingRepository
                .GetByIdAsync(booking.Id, Arg.Any<CancellationToken>())
                .Returns(booking);

            return booking;
        }

        private void GivenConfirmedOverlap(bool exists)
        {
            _bookingRepository
                .AnyAsync(Arg.Any<Specification<Booking>>(), Arg.Any<CancellationToken>())
                .Returns(exists);
        }
    }
}
