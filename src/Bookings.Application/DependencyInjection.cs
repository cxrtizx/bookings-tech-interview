using Bookings.Application.Bookings.DomainEventHandlers;
using Bookings.Application.Common;
using Bookings.Domain.Bookings.Events;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Looks for every class inheriting from AbstractValidator in this assembly
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
            services.AddScoped<IDomainEventHandler<BookingConfirmed>, BookingConfirmedDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<BookingCancelled>, BookingCancelledDomainEventHandler>();

            return services;
        }
    }
}
