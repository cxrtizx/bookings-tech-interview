using Bookings.Api.Endpoints;
using Bookings.Api.Errors;
using Bookings.Application;
using Bookings.Application.Behaviors;
using Bookings.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Bookings") ?? throw new InvalidOperationException("Cannot find \"Bookings\" connection string");
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapBookingEndpoints();

app.Run();


