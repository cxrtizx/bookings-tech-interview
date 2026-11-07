using Bookings.Application.Bookings.Commands.CreateBooking;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Bookings.Api.Endpoints
{
    public static class BookingEndpoints
    {
        public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            var group = endpointRouteBuilder.MapGroup("/api/bookings");
            group.MapPost("/", CreateBooking);

            return endpointRouteBuilder;
        }

        private static async Task<Created<CreateBookingResponse>> CreateBooking(CreateBookingRequest request, ISender sender, CancellationToken cancellationToken)
        {
            var command = new CreateBookingCommand(
                request.ResourceId,
                request.Customer?.Name ?? string.Empty,
                request.Customer?.Email ?? string.Empty,
                request.Start,
                request.End);

            var id = await sender.Send(command, cancellationToken);

            return TypedResults.Created($"/api/bookings/{id}", new CreateBookingResponse(id));
        }
    }
}
