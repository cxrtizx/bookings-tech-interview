using Bookings.Application.Ports;
using Bookings.Domain.Bookings;
using Mediator;

namespace Bookings.Application.Bookings.Commands.CreateBooking
{
    public sealed class CreateBookingCommandHandler(
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork,
        TimeProvider clock) : ICommandHandler<CreateBookingCommand, Guid>
    {
        public async ValueTask<Guid> Handle(CreateBookingCommand command, CancellationToken cancellationToken)
        {
            var resourceId = ResourceId.Create(command.ResourceId);
            var customer = Customer.Create(command.CustomerName, command.CustomerEmail);
            var dateRange = DateRange.Create(command.Start, command.End);

            // Finally
            var booking = Booking.Create(customer, dateRange, resourceId, clock);

            await bookingRepository.AddAsync(booking, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return booking.Id.Value;
        }
    }
}
