using Bookings.Application.Ports;
using Bookings.Domain.Abstractions;
using Bookings.Domain.Bookings;
using Mediator;

namespace Bookings.Application.Bookings.Queries.GetBookingById
{
    public sealed class GetBookingByIdQueryHandler(IBookingQueries bookingQueries)
        : IQueryHandler<GetBookingByIdQuery, BookingDto>
    {
        public async ValueTask<BookingDto> Handle(GetBookingByIdQuery query, CancellationToken cancellationToken)
        {
            var bookingId = BookingId.From(query.BookingId);

            return await bookingQueries.GetByIdAsync(bookingId, cancellationToken)
                ?? throw new DomainException(BookingErrors.NotFound);
        }
    }
}
