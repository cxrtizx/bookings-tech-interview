using Mediator;

namespace Bookings.Application.Bookings.Commands.CreateBooking
{
    public sealed record CreateBookingCommand(string ResourceId, string CustomerName, string CustomerEmail, DateTimeOffset Start, DateTimeOffset End) : ICommand<Guid>;
}
