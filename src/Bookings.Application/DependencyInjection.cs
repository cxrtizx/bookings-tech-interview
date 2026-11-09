using Bookings.Application.Bookings.DomainEventHandlers;
using Bookings.Application.Common;
using Bookings.Domain.Bookings.Events;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace Bookings.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Validation messages in English regardless of the server's OS culture (the API contract is in English).
            ValidatorOptions.Global.LanguageManager.Culture = CultureInfo.InvariantCulture;

            // Looks for every class inheriting from AbstractValidator in this assembly
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
            services.AddScoped<IDomainEventHandler<BookingConfirmed>, BookingConfirmedDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<BookingCancelled>, BookingCancelledDomainEventHandler>();

            return services;
        }
    }
}
