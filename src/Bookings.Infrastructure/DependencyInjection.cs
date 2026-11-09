using Bookings.Application.Ports;
using Bookings.Infrastructure.Messaging;
using Bookings.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<BookingDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("Bookings")
                ?? throw new InvalidOperationException("Couldn't find the connection string")));

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
    }
}
