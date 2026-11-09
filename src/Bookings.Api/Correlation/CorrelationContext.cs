using Bookings.Application.Ports;

namespace Bookings.Api.Correlation
{
    public sealed class CorrelationContext : ICorrelationContext
    {
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    }
}
