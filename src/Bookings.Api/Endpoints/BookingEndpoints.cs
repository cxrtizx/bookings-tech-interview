using Bookings.Application.Bookings.Commands.ConfirmBooking;
using Bookings.Application.Bookings.Commands.CreateBooking;
using Bookings.Application.Bookings.Queries.GetBookings;
using Bookings.Application.Common;
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
            group.MapGet("/", GetBookings);
            group.MapPost("/{id:guid}/confirm", ConfirmBooking);

            return endpointRouteBuilder;
        }

        private static async Task<Created<CreateBookingResponse>> CreateBooking(
            CreateBookingRequest request,
            ISender sender,
            CancellationToken cancellationToken)
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

        private static async Task<Ok<PagedResult<BookingSummaryDto>>> GetBookings(
            ISender sender,
            CancellationToken cancellationToken,
            string? resourceId = null,
            DateTimeOffset? from = null,
            DateTimeOffset? to = null,
            int page = Paging.DefaultPage,
            int pageSize = Paging.DefaultPageSize,
            string? sort = null)
        {
            var query = new GetBookingsQuery(resourceId, from, to, page, pageSize, sort);
            var result = await sender.Send(query, cancellationToken);

            return TypedResults.Ok(result);
        }

        private static async Task<Ok> ConfirmBooking(
            Guid id,
            ISender sender,
            CancellationToken cancellationToken)
        {
            var command = new ConfirmBookingCommand(id);
            await sender.Send(command, cancellationToken);

            return TypedResults.Ok();
        }
    }
}
