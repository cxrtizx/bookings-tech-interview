using Mediator;

namespace Bookings.Application.Bookings.Commands.ConfirmBooking
{
    public sealed record ConfirmBookingCommand(Guid BookingId) : ICommand;
}
