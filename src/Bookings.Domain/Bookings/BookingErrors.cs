using Bookings.Domain.Abstractions;

namespace Bookings.Domain.Bookings
{
    public static class BookingErrors
    {
        public static readonly DomainError InvalidBookingId = new("booking.invalid_booking_id", "Booking identifier cannot be empty.");

        public static readonly DomainError InvalidResourceId = new("booking.invalid_resource_id", "Resource identifier cannot be empty.");

        public static readonly DomainError ResourceIdTooLong = new("booking.resource_id_too_long", "Resource id is too long.");

        public static readonly DomainError InvalidTransition = new("booking.invalid_transition", "The transition between states is not valid.");

        public static readonly DomainError CustomerNameRequired = new("booking.customer_name_required", "Customer name is required.");

        public static readonly DomainError CustomerNameTooLong = new("booking.customer_name_too_long", "Customer name is too long.");

        public static readonly DomainError EmailAddressRequired = new("booking.email_address_required", "Email address is required.");

        public static readonly DomainError InvalidEmailAddress = new("booking.invalid_email_address", "Email address format is not valid.");

        public static readonly DomainError EmailAddressTooLong = new("booking.email_address_too_long", "Email address is too long.");

        public static readonly DomainError InvalidDateRange = new("booking.invalid_date_range", "The start time cannot be later than the end time.");

        public static readonly DomainError StartInPast = new("booking.start_in_past", "The event cannot occur in the past.");

        public static readonly DomainError Overlap = new("booking.overlap", "The booking overlaps another booking for the same resource.");

        public static readonly DomainError NotFound = new("booking.not_found", "Cannot find the booking.");

        public static readonly DomainError CancellationReasonTooLong = new("booking.cancellation_reason_too_long", "Cancellation reason is too long.");
    }
}

