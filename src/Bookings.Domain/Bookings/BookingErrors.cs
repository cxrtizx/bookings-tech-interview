using Bookings.Domain.Abstractions;

namespace Bookings.Domain.Bookings
{
    public static class BookingErrors
    {
        public static readonly DomainError InvalidId = new("booking.invalid_id", "Booking identifier cannot be empty.");

        public static readonly DomainError InvalidTransition = new("booking.invalid_transition", "The transition between states is not valid");

        public static readonly DomainError CustomerNameRequired = new("booking.customer_name_required", "Customer name is required.");

        public static readonly DomainError CustomerNameTooLong = new("booking.customer_name_too_long", "Customer name is too long.");

        public static readonly DomainError EmailAddressRequired = new("booking.email_address_required", "Email address is required.");

        public static readonly DomainError InvalidEmailAddress = new("booking.invalid_email_address", "Email address format is not valid.");
    }
}
