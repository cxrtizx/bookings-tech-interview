using Bookings.Application.Ports;
using Bookings.Infrastructure.Messaging;
using Bookings.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bookings.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<BookingDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("Bookings")
                ?? throw new InvalidOperationException("Couldn't find the connection string")));

            // RabbitMQ down is Degraded, not Unhealthy: thanks to the outbox the API keeps accepting
            // confirmations and delivers the events once the broker is back (CA10).
            services.AddHealthChecks()
                .AddDbContextCheck<BookingDbContext>("postgres")
                .AddCheck<RabbitMqHealthCheck>("rabbitmq", failureStatus: HealthStatus.Degraded);

            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IBookingQueries, BookingQueries>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<DomainEventDispatcher>();

            services.AddMassTransit(bus =>
            {
                bus.AddEntityFrameworkOutbox<BookingDbContext>(outbox =>
                {
                    outbox.UsePostgres();
                    outbox.UseBusOutbox();
                });

                bus.UsingRabbitMq((context, rabbit) =>
                {
                    rabbit.Host(configuration["RabbitMq:Host"], "/", host =>
                    {
                        host.Username(configuration["RabbitMq:Username"]!);
                        host.Password(configuration["RabbitMq:Password"]!);
                    });

                    rabbit.ConfigureEndpoints(context);
                });
            });

            services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();

            return services;
        }

        // Applies pending EF Core migrations. Meant for local/docker-compose runs (single instance);
        // in production migrations run as a separate deployment step (idempotent script or bundle).
        public static async Task MigrateBookingsDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
        {
            await using var scope = services.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<BookingDbContext>().Database.MigrateAsync(cancellationToken);
        }
    }
}
