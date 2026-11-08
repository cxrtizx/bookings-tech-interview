using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace Bookings.Application.Bookings.Queries.GetBookings
{
    public enum BookingSortField
    {
        Start,
        CreatedAt
    }

    public sealed record BookingSort(BookingSortField Field, bool IsDescending)
    {
        public static readonly BookingSort Default = new(BookingSortField.Start, IsDescending: false);

        private static readonly FrozenDictionary<string, BookingSortField> Fields =
            new Dictionary<string, BookingSortField>
            {
                ["start"] = BookingSortField.Start,
                ["createdAt"] = BookingSortField.CreatedAt
            }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        public static bool TryParse(string? value, [NotNullWhen(true)] out BookingSort? sort)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                sort = Default;
                return true;
            }

            var descending = value.StartsWith('-');
            var fieldName = descending ? value[1..] : value;

            if (Fields.TryGetValue(fieldName, out var field))
            {
                sort = new BookingSort(field, descending);
                return true;
            }

            sort = null;
            return false;
        }
    }
}
