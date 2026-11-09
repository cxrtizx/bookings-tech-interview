using Bookings.Application.Bookings.Queries;
using Bookings.Application.Bookings.Queries.GetBookings;
using Bookings.Application.Common;
using Bookings.Application.Ports;
using Bookings.Domain.Abstractions;
using Bookings.Domain.Bookings;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Bookings.Infrastructure.Persistence
{
    public sealed class BookingQueries(BookingDbContext dbContext) : IBookingQueries
    {
        private static readonly Expression<Func<Booking, BookingDto>> ToDto =
            booking => new BookingDto(
                booking.Id.Value,
                booking.Customer.Name,
                booking.Customer.Email.Value,
                booking.DateRange.Start,
                booking.DateRange.End,
                booking.CreatedAt,
                booking.ConfirmedAt,
                booking.CancelledAt,
                booking.CancellationReason,
                booking.ResourceId.Value,
                booking.Status.ToString());

        public async Task<PagedResult<BookingDto>> SearchAsync(
            Specification<Booking> specification,
            BookingSort sort,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var query = dbContext.Bookings
                .AsNoTracking()
                .Where(specification.ToExpression());

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await ApplySort(query, sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToDto)
                .ToListAsync(cancellationToken);

            return new PagedResult<BookingDto>(items, page, pageSize, totalCount);
        }

        public async Task<BookingDto?> GetByIdAsync(BookingId bookingId, CancellationToken cancellationToken)
        {
            return await dbContext.Bookings
                .AsNoTracking()
                .Where(booking => booking.Id == bookingId)
                .Select(ToDto)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static IQueryable<Booking> ApplySort(
            IQueryable<Booking> query,
            BookingSort sort) =>
            (sort.Field, sort.IsDescending) switch
            {
                (BookingSortField.CreatedAt, false) => query.OrderBy(b => b.CreatedAt).ThenBy(b => b.Id),
                (BookingSortField.CreatedAt, true) => query.OrderByDescending(b => b.CreatedAt).ThenBy(b => b.Id),
                (BookingSortField.Start, false) => query.OrderBy(b => b.DateRange.Start).ThenBy(b => b.Id),
                (BookingSortField.Start, true) => query.OrderByDescending(b => b.DateRange.Start).ThenBy(b => b.Id),
                _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, "Unsupported sort.")
            };
    }
}
