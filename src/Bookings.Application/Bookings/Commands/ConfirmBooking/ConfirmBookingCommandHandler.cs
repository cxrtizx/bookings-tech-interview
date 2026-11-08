using Bookings.Application.Ports;
using Bookings.Domain.Abstractions;
using Bookings.Domain.Bookings;
using Bookings.Domain.Bookings.Specifications;
using Mediator;

namespace Bookings.Application.Bookings.Commands.ConfirmBooking
{
    public sealed class ConfirmBookingCommandHandler(
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork,
        TimeProvider clock) : ICommandHandler<ConfirmBookingCommand>
    {
        public async ValueTask<Unit> Handle(ConfirmBookingCommand command, CancellationToken cancellationToken)
        {
            var bookingId = BookingId.From(command.BookingId);
            var booking = await bookingRepository.GetByIdAsync(bookingId, cancellationToken)
                ?? throw new DomainException(BookingErrors.NotFound);
            var overlapSpec = new ConfirmedOverlappingBookingSpec(booking.ResourceId, booking.DateRange, bookingId);
            var hasConfirmedOverlap = await bookingRepository.AnyAsync(overlapSpec, cancellationToken);

            booking.Confirm(hasConfirmedOverlap, clock);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
