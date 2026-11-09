using Bookings.Domain.Bookings;
using FluentValidation;

namespace Bookings.Application.Bookings.Commands.CancelBooking
{
    public sealed class CancelBookingCommandValidator : AbstractValidator<CancelBookingCommand>
    {
        public CancelBookingCommandValidator()
        {
            RuleFor(command => command.Reason)
                .Must(reason => reason == null || reason.Trim().Length <= Booking.CancellationReasonMaxLength)
                .WithMessage($"Cancellation reason cannot be longer than {Booking.CancellationReasonMaxLength} characters.");
        }
    }
}
