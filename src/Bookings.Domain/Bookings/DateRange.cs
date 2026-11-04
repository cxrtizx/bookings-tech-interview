using Bookings.Domain.Abstractions;

namespace Bookings.Domain.Bookings
{
    public sealed record DateRange
    {
        public DateTimeOffset Start { get; }

        public DateTimeOffset End { get; }

        private DateRange(DateTimeOffset start, DateTimeOffset end)
        {
            Start = start.ToUniversalTime();
            End = end.ToUniversalTime();
        }

        public static DateRange Create(DateTimeOffset start, DateTimeOffset end)
        {
            if (start >= end)
            {
                throw new DomainException(BookingErrors.InvalidDateRange);
            }

            return new DateRange(start, end);
        }

        public bool Overlaps(DateRange other)
        {
            return Start < other.End && End > other.Start;
        }
    }
}
