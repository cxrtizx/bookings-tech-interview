using Bookings.Domain.Abstractions;
using Bookings.Domain.Bookings;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Collections.ObjectModel;

namespace Bookings.Api.Errors
{
    public sealed class GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        private static readonly ReadOnlyDictionary<string, int> DomainErrorStatusCodes =
            new(
                new Dictionary<string, int>
                {
                    [BookingErrors.InvalidTransition.Code] = StatusCodes.Status422UnprocessableEntity,
                    [BookingErrors.Overlap.Code] = StatusCodes.Status409Conflict,
                    [BookingErrors.NotFound.Code] = StatusCodes.Status404NotFound,
                });

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var problemDetails = exception switch
            {
                ValidationException validationException => CreateValidationProblem(validationException),
                DomainException domainException => CreateDomainProblem(domainException),
                BadHttpRequestException badHttpRequestException => CreateBadHttpRequestException(),
                _ => CreateUnexpectedProblem()
            };

            if (problemDetails.Status >= 500)
            {
                logger.LogError(exception,
                    "Unexpected exception at {Method} {Path}",
                    httpContext.Request.Method,
                    httpContext.Request.Path);
            }

            httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

            var problemDetailsContext = new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception
            };

            return await problemDetailsService.TryWriteAsync(problemDetailsContext);
        }

        private static HttpValidationProblemDetails CreateValidationProblem(ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());

            var problemDetails = new HttpValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest
            };

            problemDetails.Extensions["code"] = "validation_failed";

            return problemDetails;
        }

        private static ProblemDetails CreateDomainProblem(DomainException domainException)
        {
            var status = DomainErrorStatusCodes.TryGetValue(domainException.Code, out var code)
                ? code
                : StatusCodes.Status400BadRequest;
            var title = domainException.Message;

            var problemDetails = new ProblemDetails()
            {
                Status = status,
                Title = title
            };

            problemDetails.Extensions["code"] = domainException.Code;

            return problemDetails;
        }

        private static ProblemDetails CreateBadHttpRequestException()
        {
            var problemDetails = new ProblemDetails()
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad request."
            };

            problemDetails.Extensions["code"] = "bad_request";

            return problemDetails;
        }

        private static ProblemDetails CreateUnexpectedProblem()
        {
            var problemDetails = new ProblemDetails()
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred."
            };

            problemDetails.Extensions["code"] = "internal_error";

            return problemDetails;
        }
    }
}
