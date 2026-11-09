using Bookings.Application.Ports;
using Bookings.Domain.Abstractions;
using Bookings.Domain.Bookings;
using Bookings.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Bookings.Infrastructure.Persistence
{
    public sealed class UnitOfWork(BookingDbContext dbContext, DomainEventDispatcher dispatcher) : IUnitOfWork
    {
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            var aggregates = dbContext.ChangeTracker.Entries<AggregateRoot>()
                .Select(entry => entry.Entity)
                .Where(aggregate => aggregate.DomainEvents.Count > 0)
                .ToList();

            var domainEvents = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();
            aggregates.ForEach(aggregate => aggregate.ClearDomainEvents());

            await dispatcher.DispatchAsync(domainEvents, cancellationToken);

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
