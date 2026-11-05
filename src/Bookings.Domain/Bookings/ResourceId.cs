using Bookings.Domain.Abstractions;

namespace Bookings.Domain.Bookings
{
    public sealed record ResourceId
    {
        public const int MaxLength = 200;

        public string Value { get; }

        private ResourceId(string value) => Value = value;

        public static ResourceId Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainException(BookingErrors.InvalidResourceId);
            }

            var valueNorm = value.Trim();

            if (valueNorm.Length > MaxLength)
            {
                throw new DomainException(BookingErrors.ResourceIdTooLong);
            }

            return new ResourceId(valueNorm);
        }
    }
}
