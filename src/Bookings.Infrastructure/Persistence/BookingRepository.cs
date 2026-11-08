using Bookings.Application.Ports;
using Bookings.Domain.Abstractions;
using Bookings.Domain.Bookings;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.Persistence
{
    public sealed class BookingRepository(BookingDbContext dbContext) : IBookingRepository
    {
        public async Task AddAsync(Booking booking, CancellationToken cancellationToken)
        {
            await dbContext.Bookings.AddAsync(booking, cancellationToken);
        }

        public async Task<bool> AnyAsync(Specification<Booking> spec, CancellationToken cancellationToken)
        {
            return await dbContext.Bookings.AnyAsync(spec.ToExpression(), cancellationToken);
        }

        public async Task<Booking?> GetByIdAsync(BookingId id, CancellationToken cancellationToken)
        {
            return await dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }
    }
}
