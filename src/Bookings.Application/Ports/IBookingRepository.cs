using Bookings.Domain.Abstractions;
using Bookings.Domain.Bookings;

namespace Bookings.Application.Ports
{
    public interface IBookingRepository
    {
        Task AddAsync(Booking booking, CancellationToken cancellationToken);
        Task<Booking?> GetByIdAsync(BookingId id, CancellationToken cancellationToken);
        Task<bool> AnyAsync(Specification<Booking> spec, CancellationToken cancellationToken);
    }
}
