namespace Bookings.Domain.Abstractions
{
    public sealed record DomainError(string Code, string Message);
}
