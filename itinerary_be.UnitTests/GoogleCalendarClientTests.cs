namespace itinerary_be.UnitTests;

using System.Net;
using System.Net.Http;
using System.Text;
using Google.Apis.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using itinerary_be.Modules.Auth.Exceptions;
using itinerary_be.Modules.Auth.Services;

/// <summary>
/// Unit tests for GoogleCalendarClient class
/// </summary>
public class GoogleCalendarClientTests
{
    private readonly Mock<ILogger<GoogleCalendarClient>> _mockLogger;

    public GoogleCalendarClientTests()
    {
        _mockLogger = new Mock<ILogger<GoogleCalendarClient>>();
    }

    private GoogleCalendarClient CreateClient(HttpStatusCode statusCode, string responseBody)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            });

        return new GoogleCalendarClient(_mockLogger.Object, new FakeHttpClientFactory(handler.Object));
    }

    #region GetEventsAsync Tests

    [Fact]
    public async Task GetEventsAsync_SuccessfulResponse_ReturnsMappedEvents()
    {
        // Arrange
        var body = """
        {
          "items": [
            {
              "id": "evt1",
              "summary": "Flight to Tokyo",
              "description": "Gate 22",
              "location": "Airport",
              "start": { "dateTime": "2026-08-01T10:00:00+09:00" },
              "end": { "dateTime": "2026-08-01T12:00:00+09:00" }
            },
            {
              "id": "evt2",
              "summary": "Trip to Kyoto",
              "start": { "date": "2026-08-02" },
              "end": { "date": "2026-08-03" }
            },
            {
              "id": "evt3",
              "summary": "Cancelled meetup",
              "status": "cancelled",
              "start": { "date": "2026-08-02" },
              "end": { "date": "2026-08-03" }
            }
          ]
        }
        """;
        var client = CreateClient(HttpStatusCode.OK, body);

        // Act
        var events = await client.GetEventsAsync("access-token", new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 3));

        // Assert
        Assert.Equal(2, events.Count);
        Assert.Equal("evt1", events[0].Id);
        Assert.Equal("Flight to Tokyo", events[0].Title);
        Assert.False(events[0].IsAllDay);
        Assert.Equal("evt2", events[1].Id);
        Assert.True(events[1].IsAllDay);
    }

    [Fact]
    public async Task GetEventsAsync_UnauthorizedResponse_ThrowsGoogleReauthorizationRequiredException()
    {
        // Arrange
        var client = CreateClient(HttpStatusCode.Unauthorized, """{"error": {"code": 401, "message": "Invalid Credentials"}}""");

        // Act & Assert
        await Assert.ThrowsAsync<GoogleReauthorizationRequiredException>(
            () => client.GetEventsAsync("bad-token", new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 3)));
    }

    [Fact]
    public async Task GetEventsAsync_ServerError_ThrowsGoogleCalendarApiException()
    {
        // Arrange
        var client = CreateClient(HttpStatusCode.InternalServerError, """{"error": {"code": 500, "message": "Backend Error"}}""");

        // Act & Assert
        await Assert.ThrowsAsync<GoogleCalendarApiException>(
            () => client.GetEventsAsync("access-token", new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 3)));
    }

    #endregion

    private class FakeHttpClientFactory : Google.Apis.Http.IHttpClientFactory
    {
        private readonly HttpMessageHandler _innerHandler;

        public FakeHttpClientFactory(HttpMessageHandler innerHandler)
        {
            _innerHandler = innerHandler;
        }

        public ConfigurableHttpClient CreateHttpClient(CreateHttpClientArgs args) =>
            new(new ConfigurableMessageHandler(_innerHandler));
    }
}
