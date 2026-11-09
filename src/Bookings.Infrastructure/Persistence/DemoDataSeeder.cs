using Bookings.Domain.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Infrastructure.Persistence
{
    // Demo data for docker-compose runs (Database:SeedDemoData=true). Bookings are built through the
    // aggregate so they respect every domain rule. Dates are relative to "tomorrow" so they are always
    // in the future. Room-A is prepared to reproduce CA3 (overlap) and CA4 (adjacency) from the UI.
    public static class DemoDataSeeder
    {
        public static async Task SeedDemoDataAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
        {
            await using var scope = services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
            var clock = scope.ServiceProvider.GetRequiredService<TimeProvider>();

            if (await dbContext.Bookings.AnyAsync(cancellationToken))
            {
                return;
            }

            var tomorrow = new DateTimeOffset(clock.GetUtcNow().UtcDateTime.Date.AddDays(1), TimeSpan.Zero);

            Booking Create(string resource, string name, int dayOffset, int startHour, int startMinute, int durationMinutes)
            {
                var start = tomorrow.AddDays(dayOffset).AddHours(startHour).AddMinutes(startMinute);
                return Booking.Create(
                    Customer.Create(name, $"{name.ToLowerInvariant()}@example.com"),
                    DateRange.Create(start, start.AddMinutes(durationMinutes)),
                    ResourceId.Create(resource),
                    clock);
            }

            // CA3/CA4 scenario: Ana confirmed 10:00-11:00; confirming Bruno (10:30) fails, Carla (11:00) succeeds.
            var ana = Create("Room-A", "Ana", 0, 10, 0, 60);
            ana.Confirm(hasConfirmedOverlap: false, clock);
            var bruno = Create("Room-A", "Bruno", 0, 10, 30, 60);
            var carla = Create("Room-A", "Carla", 0, 11, 0, 60);

            var diego = Create("Room-B", "Diego", 0, 9, 0, 90);
            diego.Confirm(hasConfirmedOverlap: false, clock);
            var elena = Create("Room-B", "Elena", 1, 16, 0, 60);
            var fran = Create("Room-B", "Fran", 1, 12, 0, 60);
            fran.Cancel("Customer called to cancel", clock);

            var others = Enumerable.Range(0, 6)
                .Select(day => Create("Desk-12", $"Guest{day + 1}", day + 2, 9, 0, 480))
                .ToList();

            var bookings = new List<Booking> { ana, bruno, carla, diego, elena, fran };
            bookings.AddRange(others);

            // Seed data must not produce integration events/notifications: events are cleared and the
            // DbContext is saved directly instead of going through the UnitOfWork dispatcher.
            bookings.ForEach(booking => booking.ClearDomainEvents());
            dbContext.Bookings.AddRange(bookings);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
