using Bookings.Domain.Abstractions;
using Bookings.Domain.Bookings.Events;

namespace Bookings.Domain.Bookings
{
    public sealed class Booking : AggregateRoot
    {
        public BookingId Id { get; private set; } = null!;
        public Customer Customer { get; private set; } = null!;
        public DateRange DateRange { get; private set; } = null!;
        public ResourceId ResourceId { get; private set; } = null!;
        public BookingStatus Status { get; private set; }

        private Booking() { } // EF Core
        private Booking(
            Customer customer,
            DateRange dateRange,
            ResourceId resourceId)
        {
            Customer = customer;
            DateRange = dateRange;
            ResourceId = resourceId;
            Status = BookingStatus.Pending;
            Id = BookingId.New();
        }

        public static Booking Create(Customer customer, DateRange dateRange, ResourceId resourceId, TimeProvider clock)
        {
            if (dateRange.Start <= clock.GetUtcNow())
            {
                throw new DomainException(BookingErrors.StartInPast);
            }

            var booking = new Booking(customer, dateRange, resourceId);
            booking.Raise(new BookingCreated(booking.Id));

            return booking;
        }
    }
}
