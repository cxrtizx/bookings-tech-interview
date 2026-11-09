using Bookings.Application.Ports;
using Bookings.Domain.Abstractions;
using Bookings.Domain.Bookings;
using Mediator;

namespace Bookings.Application.Bookings.Commands.CancelBooking
{
    public sealed class CancelBookingCommandHandler(
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork,
        TimeProvider clock) : ICommandHandler<CancelBookingCommand>
    {
        public async ValueTask<Unit> Handle(CancelBookingCommand command, CancellationToken cancellationToken)
        {
            var bookingId = BookingId.From(command.BookingId);
            var booking = await bookingRepository.GetByIdAsync(bookingId, cancellationToken)
                ?? throw new DomainException(BookingErrors.NotFound);

            booking.Cancel(command.Reason, clock);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
