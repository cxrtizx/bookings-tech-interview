using Bookings.Application.Ports;
using Bookings.Domain.Bookings;

namespace Bookings.Infrastructure.Persistence
{
    public sealed class BookingRepository(BookingDbContext dbContext) : IBookingRepository
    {
        public async Task AddAsync(Booking booking, CancellationToken cancellationToken)
        {
            await dbContext.Bookings.AddAsync(booking, cancellationToken);
        }
    }
}
