namespace Bookings.Api.Endpoints
{
    // Represents the request json
    public sealed record CreateBookingRequest(
        CustomerRequest Customer,
        DateTimeOffset Start,
        DateTimeOffset End,
        string ResourceId);

    public sealed record CustomerRequest(string Name, string Email);
}
