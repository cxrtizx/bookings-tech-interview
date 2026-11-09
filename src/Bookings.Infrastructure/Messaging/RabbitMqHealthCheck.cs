using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace Bookings.Infrastructure.Messaging
{
    // MassTransit's own "masstransit-bus" check only reports the bus state; on a publish-only bus
    // (no consumers, as in this API) it stays healthy even with the broker down. This check opens
    // a real AMQP connection so /health reflects whether RabbitMQ is reachable.
    public sealed class RabbitMqHealthCheck(IConfiguration configuration) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMq:Host"]!,
                UserName = configuration["RabbitMq:Username"]!,
                Password = configuration["RabbitMq:Password"]!,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(3)
            };

            try
            {
                await using var connection = await factory.CreateConnectionAsync("bookings-api-health", cancellationToken);

                return connection.IsOpen
                    ? HealthCheckResult.Healthy()
                    : new HealthCheckResult(context.Registration.FailureStatus, "RabbitMQ connection is not open.");
            }
            catch (Exception exception)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, "RabbitMQ is unreachable.", exception);
            }
        }
    }
}
