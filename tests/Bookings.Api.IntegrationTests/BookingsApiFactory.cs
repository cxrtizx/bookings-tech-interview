using Bookings.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;

namespace Bookings.Api.IntegrationTests
{
    // Real API pipeline + real PostgreSQL (Testcontainers) + MassTransit test harness instead of RabbitMQ.
    // The EF Core outbox is kept, so the test exercises the same path as production:
    // aggregate change + OutboxMessage in one transaction, then delivery to the bus.
    public sealed class BookingsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18").Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Bookings"] = _postgres.GetConnectionString()
                }));

            builder.ConfigureTestServices(services => services.AddMassTransitTestHarness(bus =>
            {
                bus.AddConsumer<BookingConfirmedProbeConsumer>();

                bus.AddEntityFrameworkOutbox<BookingDbContext>(outbox =>
                {
                    outbox.UsePostgres();
                    outbox.UseBusOutbox();
                    outbox.QueryDelay = TimeSpan.FromMilliseconds(100);
                });
            }));
        }

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();

            // Migrate before the host starts: the outbox delivery service queries its tables as soon as it runs.
            var options = new DbContextOptionsBuilder<BookingDbContext>()
                .UseNpgsql(_postgres.GetConnectionString())
                .Options;

            await using var dbContext = new BookingDbContext(options);
            await dbContext.Database.MigrateAsync();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await base.DisposeAsync();
            await _postgres.DisposeAsync();
        }
    }
}
