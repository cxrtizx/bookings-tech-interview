using FluentValidation;
using FluentValidation.Results;
using Mediator;

namespace Bookings.Application.Behaviors
{
    public sealed class ValidationBehavior<TMessage, TResponse>(
        IEnumerable<IValidator<TMessage>> validators)
        : IPipelineBehavior<TMessage, TResponse> where TMessage : IMessage
    {
        public async ValueTask<TResponse> Handle(
            TMessage message,
            MessageHandlerDelegate<TMessage, TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!validators.Any())
            {
                return await next(message, cancellationToken);
            }

            var errors = new List<ValidationFailure>();

            foreach (var validator in validators)
            {
                var result = await validator.ValidateAsync(message, cancellationToken);
                if (!result.IsValid)
                {
                    errors.AddRange(result.Errors);
                }
            }

            if (errors.Count > 0)
            {
                throw new ValidationException(errors);
            }
            else
            {
                return await next(message, cancellationToken);
            }
        }
    }
}

