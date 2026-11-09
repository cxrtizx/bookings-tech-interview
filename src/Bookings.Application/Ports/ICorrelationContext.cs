namespace Bookings.Application.Ports
{
    public interface ICorrelationContext
    {
        string CorrelationId { get; }
    }
}
