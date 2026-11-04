using Bookings.Domain.Abstractions;

namespace Bookings.Domain.Bookings
{
    public sealed record ResourceId
    {
        public string Value { get; }

        private ResourceId(string value) => Value = value;

        public static ResourceId Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainException(BookingErrors.InvalidResourceId);
            }

            return new ResourceId(value.Trim());
        }
    }
}
