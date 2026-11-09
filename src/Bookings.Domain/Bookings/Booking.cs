using Bookings.Domain.Abstractions;
using Bookings.Domain.Bookings.Events;

namespace Bookings.Domain.Bookings
{
    public sealed class Booking : AggregateRoot
    {
        public const int CancellationReasonMaxLength = 100;

        public BookingId Id { get; private set; } = null!;
        public Customer Customer { get; private set; } = null!;
        public DateRange DateRange { get; private set; } = null!;
        public ResourceId ResourceId { get; private set; } = null!;
        public BookingStatus Status { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? ConfirmedAt { get; private set; }
        public DateTimeOffset? CancelledAt { get; private set; }
        public string? CancellationReason { get; private set; }

        private Booking() { } // Mandatory for EF Core
        private Booking(
            Customer customer,
            DateRange dateRange,
            ResourceId resourceId,
            DateTimeOffset now)
        {
            Customer = customer;
            DateRange = dateRange;
            ResourceId = resourceId;
            Status = BookingStatus.Pending;
            CreatedAt = now;
            Id = BookingId.New();
        }

        public static Booking Create(Customer customer, DateRange dateRange, ResourceId resourceId, TimeProvider clock)
        {
            var now = clock.GetUtcNow();
            if (dateRange.Start <= now)
            {
                throw new DomainException(BookingErrors.StartInPast);
            }

            var booking = new Booking(customer, dateRange, resourceId, now);
            booking.Raise(new BookingCreated(booking.Id));

            return booking;
        }

        public void Confirm(bool hasConfirmedOverlap, TimeProvider clock)
        {
            if (Status != BookingStatus.Pending)
            {
                throw new DomainException(BookingErrors.InvalidTransition);
            }

            if (hasConfirmedOverlap)
            {
                throw new DomainException(BookingErrors.Overlap);
            }

            var now = clock.GetUtcNow();
            ConfirmedAt = now;
            Status = BookingStatus.Confirmed;

            Raise(new BookingConfirmed(Id, ResourceId, DateRange, Customer.Email, now));
        }

        public void Cancel(string? reason, TimeProvider clock)
        {
            // New states may be added in the future
            if (Status is not (BookingStatus.Pending or BookingStatus.Confirmed))
            {
                throw new DomainException(BookingErrors.InvalidTransition);
            }

            var normalizedReason = string.IsNullOrWhiteSpace(reason)
                ? null
                : reason.Trim();

            if (normalizedReason?.Length > CancellationReasonMaxLength)
            {
                throw new DomainException(BookingErrors.CancellationReasonTooLong);
            }

            var now = clock.GetUtcNow();
            Status = BookingStatus.Cancelled;
            CancelledAt = now;
            CancellationReason = normalizedReason;

            Raise(new BookingCancelled(Id, ResourceId, DateRange, Customer.Email, now, normalizedReason));
        }
    }
}
