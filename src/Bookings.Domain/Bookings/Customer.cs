using Bookings.Domain.Abstractions;

namespace Bookings.Domain.Bookings
{
    public sealed record Customer
    {
        public const int NameMaxLength = 100;

        public string Name { get; }

        public Email Email { get; }

        private Customer(string name, Email email)
        {
            Name = name;
            Email = email;
        }

        public static Customer Create(string name, string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException(BookingErrors.CustomerNameRequired);
            }

            name = name.Trim();

            if (name.Length > NameMaxLength)
            {
                throw new DomainException(BookingErrors.CustomerNameTooLong);
            }

            var email = Email.Create(emailAddress);

            return new Customer(name, email);
        }
    }
}
