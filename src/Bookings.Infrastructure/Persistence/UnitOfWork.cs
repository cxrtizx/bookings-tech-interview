using Bookings.Application.Ports;

namespace Bookings.Infrastructure.Persistence
{
    public sealed class UnitOfWork(BookingDbContext dbContext) : IUnitOfWork
    {
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
