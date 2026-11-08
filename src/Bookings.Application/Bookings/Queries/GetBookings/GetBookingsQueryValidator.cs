using Bookings.Domain.Bookings;
using FluentValidation;

namespace Bookings.Application.Bookings.Queries.GetBookings
{
    public sealed class GetBookingsQueryValidator : AbstractValidator<GetBookingsQuery>
    {
        public const int MaxPageSize = 100;

        public GetBookingsQueryValidator()
        {
            RuleFor(query => query.Page)
                .GreaterThanOrEqualTo(1);

            RuleFor(query => query.PageSize)
                .InclusiveBetween(1, MaxPageSize);

            RuleFor(query => query.ResourceId)
                .MaximumLength(ResourceId.MaxLength);

            RuleFor(query => query.To)
                .GreaterThan(query => query.From)
                .When(query => query.From.HasValue && query.To.HasValue);

            RuleFor(query => query.Sort)
                .Must(sort => BookingSort.TryParse(sort, out _))
                .WithMessage("Sort must be start or createdAt, optionally prefixed with '-' for descending.");
        }
    }
}
