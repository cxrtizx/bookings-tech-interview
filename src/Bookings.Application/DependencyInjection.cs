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

            return services;
        }
    }
}
