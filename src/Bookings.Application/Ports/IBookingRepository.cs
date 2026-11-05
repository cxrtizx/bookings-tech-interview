using Bookings.Domain.Bookings;

namespace Bookings.Application.Ports
{
    public interface IBookingRepository
    {
        Task AddAsync(Booking booking, CancellationToken cancellationToken);
    }
}
