using Bookings.Notifications.Worker.Consumers;
using Bookings.Notifications.Worker.Notifications;
using Bookings.Notifications.Worker.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Structured logs: every entry is a JSON object and scopes (CorrelationId, EventId) become searchable fields.
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);

var connectionString = builder.Configuration.GetConnectionString("Notifications")
    ?? throw new InvalidOperationException("Cannot find \"Notifications\" connection string");

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContext<NotificationsDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", NotificationsDbContext.Schema)));
builder.Services.AddScoped<NotificationRecorder>();

builder.Services.AddMassTransit(bus =>
{
    // Queues named after the owning service: notifications-booking-confirmed, notifications-booking-cancelled.
    bus.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("notifications", false));

    bus.AddConsumer<BookingConfirmedConsumer>();
    bus.AddConsumer<BookingCancelledConsumer>();

    bus.UsingRabbitMq((context, rabbit) =>
    {
        rabbit.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        {
            host.Username(builder.Configuration["RabbitMq:Username"]!);
            host.Password(builder.Configuration["RabbitMq:Password"]!);
        });

        // After the last retry fails, MassTransit moves the message to the <queue>_error queue (DLQ).
        rabbit.UseMessageRetry(retry => retry.Intervals(
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(15)));

        rabbit.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
