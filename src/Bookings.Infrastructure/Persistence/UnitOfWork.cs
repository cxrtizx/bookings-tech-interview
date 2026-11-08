using Bookings.Application.Ports;
using Bookings.Domain.Abstractions;
using Bookings.Domain.Bookings;
using Bookings.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Bookings.Infrastructure.Persistence
{
    public sealed class UnitOfWork(BookingDbContext dbContext) : IUnitOfWork
    {
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await dbContext.SaveChangesAsync(cancellationToken);
            }
            // Another booking has been confirmed previously for the same resource and time.
            // Exception translated to business errors
            catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.ExclusionViolation,
                ConstraintName: BookingConfiguration.NoConfirmedOverlapConstraint
            })
            {
                throw new DomainException(BookingErrors.Overlap, exception);
            }
        }
    }
}
