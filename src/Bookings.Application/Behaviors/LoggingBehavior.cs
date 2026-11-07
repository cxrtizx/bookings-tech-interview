using Bookings.Domain.Abstractions;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Bookings.Application.Behaviors
{
    public sealed class LoggingBehavior<TMessage, TResponse>(
        ILogger<LoggingBehavior<TMessage, TResponse>> logger) :
        IPipelineBehavior<TMessage, TResponse> where TMessage : IMessage
    {
        public async ValueTask<TResponse> Handle(
            TMessage message, MessageHandlerDelegate<TMessage,
            TResponse> next,
            CancellationToken cancellationToken)
        {
            var messageName = typeof(TMessage).Name;
            logger.LogInformation("Handling {MessageName} ...", messageName);
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var response = await next(message, cancellationToken);
                logger.LogInformation(
                    "{MessageName} handled. Elapsed time: {ElapsedMilliseconds:0.00}ms",
                    messageName,
                    stopwatch.Elapsed.TotalMilliseconds);

                return response;
            }
            catch (ValidationException validationException)
            {
                logger.LogWarning(
                    "Validation error: {MessageName} failed with {ErrorCount} error(s) in {PropertyNames}.",
                    messageName,
                    validationException.Errors.Count(),
                    validationException.Errors.Select(e => e.PropertyName).Distinct());

                throw;
            }
            catch (DomainException domainException)
            {
                logger.LogWarning(
                    "Domain error: {MessageName} rejected. Cause: {ErrorCode}",
                    messageName,
                    domainException.Code);

                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Unexpected exception while handling {MessageName}",
                    messageName);

                throw;
            }
        }
    }
}
