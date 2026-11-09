using Microsoft.EntityFrameworkCore;

namespace Bookings.Notifications.Worker.Persistence
{
    public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : DbContext(options)
    {
        public const string Schema = "notifications";
        public const string EventIdUniqueIndex = "IX_Notifications_EventId";

        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Own schema: the worker never shares tables or migration history with the bookings API.
            modelBuilder.HasDefaultSchema(Schema);

            modelBuilder.Entity<Notification>(notification =>
            {
                notification.ToTable("Notifications");
                notification.HasKey(n => n.Id);
                notification.Property(n => n.Id).ValueGeneratedNever();

                // Idempotency guarantee: the same integration event can only be recorded once.
                notification.HasIndex(n => n.EventId)
                    .IsUnique()
                    .HasDatabaseName(EventIdUniqueIndex);

                notification.Property(n => n.EventType).HasMaxLength(100);
                notification.Property(n => n.Recipient).HasMaxLength(100);
                notification.Property(n => n.Message).HasMaxLength(500);
                notification.Property(n => n.CorrelationId).HasMaxLength(128);
            });
        }
    }
}
