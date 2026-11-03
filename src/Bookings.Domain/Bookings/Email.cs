using Bookings.Domain.Abstractions;
using System.Text.RegularExpressions;

namespace Bookings.Domain.Bookings
{
    public sealed partial record Email
    {
        [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
        private static partial Regex Pattern();

        public string Value { get; }

        private Email(string emailAddress)
        {
            Value = emailAddress;
        }

        public static Email Create(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                throw new DomainException(BookingErrors.EmailAddressRequired);
            }

            emailAddress = emailAddress.Trim().ToLowerInvariant();

            if (!Pattern().IsMatch(emailAddress))
            {
                throw new DomainException(BookingErrors.InvalidEmailAddress);
            }

            return new(emailAddress);
        }
    }
}
