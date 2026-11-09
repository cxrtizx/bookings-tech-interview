using System.Globalization;
using Bookings.Domain.Bookings;

namespace Bookings.Domain.Tests.TestData
{
    // Object Mother: builds bookings in a given state through the public API of the aggregate,
    // so tests never bypass the domain rules to reach a state.
    internal static class BookingMother
    {
        public static readonly DateTimeOffset Now = new(2027, 1, 1, 9, 0, 0, TimeSpan.Zero);

        private static readonly DateOnly BookingDay = new(2027, 1, 10);

        public static DateTimeOffset At(string time) =>
            new(BookingDay.ToDateTime(TimeOnly.Parse(time, CultureInfo.InvariantCulture)), TimeSpan.Zero);

        public static DateRange Range(string start, string end) => DateRange.Create(At(start), At(end));

        public static Booking Pending(
            TimeProvider clock,
            string resourceId = "room-A",
            string start = "10:00",
            string end = "11:00") =>
            Booking.Create(
                Customer.Create("Ana", "ana@example.com"),
                Range(start, end),
                ResourceId.Create(resourceId),
                clock);

        public static Booking Confirmed(
            TimeProvider clock,
            string resourceId = "room-A",
            string start = "10:00",
            string end = "11:00")
        {
            var booking = Pending(clock, resourceId, start, end);
            booking.Confirm(hasConfirmedOverlap: false, clock);

            return booking;
        }

        public static Booking Cancelled(TimeProvider clock)
        {
            var booking = Pending(clock);
            booking.Cancel(reason: null, clock);

            return booking;
        }

        public static Booking InStatus(BookingStatus status, TimeProvider clock) => status switch
        {
            BookingStatus.Pending => Pending(clock),
            BookingStatus.Confirmed => Confirmed(clock),
            BookingStatus.Cancelled => Cancelled(clock),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported status.")
        };
    }
}
