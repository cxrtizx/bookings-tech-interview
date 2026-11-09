using Bookings.Notifications.Worker.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Bookings.Notifications.Worker.Notifications
{
    // Records a notification at most once per integration event (deduplication by EventId).
    public sealed class NotificationRecorder(
        NotificationsDbContext dbContext,
        ILogger<NotificationRecorder> logger)
    {
        public async Task<bool> TryRecordAsync(Notification notification, CancellationToken cancellationToken)
        {
            // Common case: a redelivery of an event that was already processed.
            if (await dbContext.Notifications.AnyAsync(n => n.EventId == notification.EventId, cancellationToken))
            {
                logger.LogInformation("Duplicate event {EventId} ignored", notification.EventId);
                return false;
            }

            dbContext.Notifications.Add(notification);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: NotificationsDbContext.EventIdUniqueIndex
            })
            {
                // Race: the same event was delivered concurrently and another consumer recorded it first.
                // Treated as success so the message is acknowledged instead of ending up in the error queue.
                dbContext.Entry(notification).State = EntityState.Detached;
                logger.LogInformation("Duplicate event {EventId} ignored (concurrent delivery)", notification.EventId);
                return false;
            }
        }
    }
}
