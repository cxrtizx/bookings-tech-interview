using System.Net;
using System.Net.Http.Json;
using Bookings.Contracts;
using MassTransit.Testing;

namespace Bookings.Api.IntegrationTests
{
    public sealed class ConfirmBookingTests(BookingsApiFactory factory) : IClassFixture<BookingsApiFactory>
    {
        [Fact]
        public async Task ConfirmBooking_PublishesBookingConfirmedV1WithTheRequestCorrelationId()
        {
            var client = factory.CreateClient();
            var harness = factory.Services.GetTestHarness();
            const string correlationId = "integration-test-correlation";

            var createResponse = await client.PostAsJsonAsync("/api/bookings", new
            {
                customer = new { name = "Ana", email = "ana@example.com" },
                start = DateTimeOffset.UtcNow.AddDays(30),
                end = DateTimeOffset.UtcNow.AddDays(30).AddHours(1),
                resourceId = "room-integration"
            });
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var created = await createResponse.Content.ReadFromJsonAsync<CreatedBooking>();
            var bookingId = created!.Id;

            using var confirmRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/bookings/{bookingId}/confirm");
            confirmRequest.Headers.Add("X-Correlation-Id", correlationId);
            var confirmResponse = await client.SendAsync(confirmRequest);

            Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

            var message = await WaitForBookingConfirmedAsync(harness, bookingId, TimeSpan.FromSeconds(10));
            Assert.NotNull(message);
            Assert.Equal(correlationId, message.CorrelationId);
            Assert.Equal("room-integration", message.ResourceId);
            Assert.Equal("ana@example.com", message.CustomerEmail);
        }

        // With the bus outbox, Publish only stores the message in OutboxMessage (same transaction as the booking).
        // The delivery service later forwards the already-serialized message to the message type's exchange, so the
        // reliable check is what a subscriber actually consumes. The message waits in the database until the next
        // poll, so the bus looks idle for a moment: poll instead of relying on a single harness query.
        private static async Task<BookingConfirmedV1?> WaitForBookingConfirmedAsync(
            ITestHarness harness,
            Guid bookingId,
            TimeSpan timeout)
        {
            var probe = harness.GetConsumerHarness<BookingConfirmedProbeConsumer>();
            var deadline = DateTime.UtcNow + timeout;

            while (DateTime.UtcNow < deadline)
            {
                var consumed = probe.Consumed
                    .Select<BookingConfirmedV1>(candidate => candidate.Context.Message.BookingId == bookingId)
                    .FirstOrDefault();

                if (consumed is not null)
                {
                    return consumed.Context.Message;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            return null;
        }

        private sealed record CreatedBooking(Guid Id);
    }
}
