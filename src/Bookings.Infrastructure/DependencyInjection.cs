using Bookings.Application.Ports;
using Bookings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<BookingDbContext>(options => options.UseNpgsql(connectionString));

            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IBookingQueries, BookingQueries>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<DomainEventDispatcher>();

            return services;
        }
    }
}
