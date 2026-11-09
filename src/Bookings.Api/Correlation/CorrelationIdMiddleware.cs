namespace Bookings.Api.Correlation
{
    public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        public const string HeaderName = "X-Correlation-Id";
        private const int MaxLength = 128;

        public async Task InvokeAsync(HttpContext httpContext, CorrelationContext correlationContext)
        {
            var incoming = httpContext.Request.Headers[HeaderName].ToString();
            if (!string.IsNullOrWhiteSpace(incoming) && incoming.Length <= MaxLength)
            {
                correlationContext.CorrelationId = incoming;
            }

            httpContext.Response.OnStarting(() =>
            {
                httpContext.Response.Headers[HeaderName] = correlationContext.CorrelationId;
                return Task.CompletedTask;
            });

            using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationContext.CorrelationId }))
            {
                await next(httpContext);
            }
        }
    }
}
