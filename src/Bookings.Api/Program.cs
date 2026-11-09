using Bookings.Api.Correlation;
using Bookings.Api.Endpoints;
using Bookings.Api.Errors;
using Bookings.Application;
using Bookings.Application.Behaviors;
using Bookings.Application.Ports;
using Bookings.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddProblemDetails();
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

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapBookingEndpoints();

app.Run();


