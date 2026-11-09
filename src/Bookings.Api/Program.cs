using Bookings.Api.Correlation;
using Bookings.Api.Endpoints;
using Bookings.Api.Errors;
using Bookings.Application;
using Bookings.Application.Behaviors;
using Bookings.Application.Ports;
using Bookings.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
// Every ProblemDetails carries the request's correlation id, so a user-facing error can be traced in the logs.
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
    context.ProblemDetails.Extensions["correlationId"] =
        context.HttpContext.RequestServices.GetService<ICorrelationContext>()?.CorrelationId);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddScoped<CorrelationContext>();
builder.Services.AddScoped<ICorrelationContext>(sp => sp.GetRequiredService<CorrelationContext>());

builder.Services.AddMediator(
    options =>
    {
        options.ServiceLifetime = ServiceLifetime.Scoped;
        options.PipelineBehaviors =
        [
            typeof(LoggingBehavior<,>),
            typeof(ValidationBehavior<,>)
        ];
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Allowed origins come from configuration so each environment can set its own frontend URL.
// Exposed headers: browsers hide response headers from cross-origin JavaScript unless they are listed.
builder.Services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithExposedHeaders("Location", CorrelationIdMiddleware.HeaderName)));

builder.Logging.AddSimpleConsole(options => options.IncludeScopes = true);

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.MapBookingEndpoints();

// Reports each check (postgres, masstransit-bus) as JSON; returns 503 when any of them is unhealthy.
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = (context, report) => context.Response.WriteAsJsonAsync(new
    {
        status = report.Status.ToString(),
        checks = report.Entries.Select(entry => new
        {
            name = entry.Key,
            status = entry.Value.Status.ToString(),
            description = entry.Value.Description
        })
    })
});

app.Run();

// Exposes the implicit Program class so integration tests can use WebApplicationFactory<Program>.
public partial class Program;
