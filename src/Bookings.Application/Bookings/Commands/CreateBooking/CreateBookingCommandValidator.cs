using Bookings.Domain.Bookings;
using FluentValidation;

namespace Bookings.Application.Bookings.Commands.CreateBooking
{
    public sealed class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingCommandValidator(TimeProvider clock)
        {
            RuleFor(command => command.ResourceId)
                .NotEmpty()
                .MaximumLength(ResourceId.MaxLength);

            RuleFor(command => command.CustomerName)
                .NotEmpty()
                .MaximumLength(Customer.NameMaxLength);

            RuleFor(command => command.CustomerEmail)
                .NotEmpty()
                .MaximumLength(Email.MaxLength)
                .EmailAddress();

            RuleFor(command => command.Start)
                .GreaterThan(_ => clock.GetUtcNow());

            RuleFor(command => command.End)
                .GreaterThan(command => command.Start);
        }
    }
}

