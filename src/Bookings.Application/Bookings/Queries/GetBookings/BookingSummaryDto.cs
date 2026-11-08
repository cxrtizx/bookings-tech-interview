namespace Bookings.Application.Bookings.Queries.GetBookings
{
    public sealed record BookingSummaryDto(
        Guid Id,
        string CustomerName,
        string CustomerEmail,
        DateTimeOffset Start,
        DateTimeOffset End,
        DateTimeOffset CreatedAt,
        string ResourceId,
        string Status);
}
