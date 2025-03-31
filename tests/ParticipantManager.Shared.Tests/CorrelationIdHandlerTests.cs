using Microsoft.Azure.Functions.Worker;
using Moq;
using System.Net;

namespace ParticipantManager.Shared.Tests;

public class CorrelationIdHandlerTests
{
    private const string TestCorrelationId = "test-correlation-id";
    private const string ExistingCorrelationId = "existing-correlation-id";
    private const string TestUrl = "https://example.com";
    private const string CorrelationIdHeader = "X-Correlation-ID";

    private readonly Mock<FunctionContext> _mockFunctionContext;
    private readonly Mock<FunctionContextAccessor> _mockAccessor;
    private readonly CorrelationIdHandler _correlationIdHandler;
    private readonly HttpClient _httpClient;

    public CorrelationIdHandlerTests()
    {
        _mockFunctionContext = new Mock<FunctionContext>();
        _mockAccessor = new Mock<FunctionContextAccessor>();
        _mockAccessor.Setup(x => x.FunctionContext).Returns(_mockFunctionContext.Object);
        _correlationIdHandler = new CorrelationIdHandler(_mockAccessor.Object) { InnerHandler = new HttpClientHandler() };
        _httpClient = new HttpClient(_correlationIdHandler);
    }

    [Fact]
    public async Task SendAsync_CorrelationIdPresent_AddsHeaderToRequest()
    {
        // Arrange
        _mockFunctionContext.Setup(x => x.Items).Returns(new Dictionary<object, object> { { CorrelationIdHeader, TestCorrelationId } });
        var request = new HttpRequestMessage(HttpMethod.Get, TestUrl);

        // Act
        var result = await _httpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(request.Headers.Contains(CorrelationIdHeader));
        Assert.Equal(TestCorrelationId, request.Headers.GetValues(CorrelationIdHeader).First());
    }

    [Fact]
    public async Task SendAsync_CorrelationIdAlreadyExists_DoesNotAddHeader()
    {
        // Arrange
        _mockFunctionContext.Setup(x => x.Items).Returns(new Dictionary<object, object> { { CorrelationIdHeader, ExistingCorrelationId } });
        var request = new HttpRequestMessage(HttpMethod.Get, TestUrl);
        request.Headers.Add(CorrelationIdHeader, ExistingCorrelationId);

        // Act
        var result = await _httpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var headerValues = request.Headers.GetValues(CorrelationIdHeader).ToList();
        Assert.Single(headerValues);
        Assert.Equal(ExistingCorrelationId, headerValues[0]);
    }

    [Fact]
    public async Task SendAsync_NoCorrelationIdInContext_DoesNotAddHeader()
    {
        // Arrange
        _mockFunctionContext.Setup(x => x.Items).Returns(new Dictionary<object, object>());
        var request = new HttpRequestMessage(HttpMethod.Get, TestUrl);

        // Act
        var result = await _httpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.False(request.Headers.Contains(CorrelationIdHeader));
    }
}
