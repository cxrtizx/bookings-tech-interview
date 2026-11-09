using Mediator;

namespace Bookings.Application.Bookings.Commands.CancelBooking
{
    public sealed record CancelBookingCommand(Guid BookingId, string? Reason) : ICommand;
}
