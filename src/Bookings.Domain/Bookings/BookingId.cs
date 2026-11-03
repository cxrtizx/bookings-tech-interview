using Bookings.Domain.Abstractions;

namespace Bookings.Domain.Bookings
{
    public sealed record BookingId
    {
        public Guid Value { get; }

        private BookingId(Guid value) => Value = value;

        public static BookingId New() => new(Guid.NewGuid());

        public static BookingId From(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new DomainException(BookingErrors.InvalidId);
            }

            return new BookingId(value);
        }
    }
}
