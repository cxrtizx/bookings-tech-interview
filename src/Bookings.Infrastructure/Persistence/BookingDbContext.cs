using Bookings.Domain.Bookings;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.Persistence
{
    public sealed class BookingDbContext(DbContextOptions<BookingDbContext> options) : DbContext(options)
    {
        public DbSet<Booking> Bookings => Set<Booking>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);
        }
    }
}
