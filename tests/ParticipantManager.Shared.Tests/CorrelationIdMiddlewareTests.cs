using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Sinks.InMemory;

namespace ParticipantManager.Shared;

public class CorrelationIdMiddlewareTests
{
    private const string CorrelationIdHeader = "X-Correlation-ID";

    private readonly Mock<FunctionContext> _mockContext;
    private readonly Mock<FunctionExecutionDelegate> _mockNext;
    private readonly Mock<FunctionContextAccessor> _mockAccessor;

    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private readonly CorrelationIdMiddleware _middleware;

    public CorrelationIdMiddlewareTests()
    {
        _mockContext = new Mock<FunctionContext>();
        _mockNext = new Mock<FunctionExecutionDelegate>();
        _mockAccessor = new Mock<FunctionContextAccessor>();

        Log.Logger = new LoggerConfiguration()
            .WriteTo.InMemory()
            .CreateLogger();

        _logger = new LoggerFactory()
            .AddSerilog(Log.Logger)
            .CreateLogger<CorrelationIdMiddleware>();

        _mockContext.Setup(c => c.InstanceServices)
            .Returns(new ServiceCollection()
            .AddSingleton(_logger)
            .BuildServiceProvider());

        var items = new Dictionary<object, object>();
        _mockContext.Setup(c => c.Items).Returns(items);

        _middleware = new CorrelationIdMiddleware(_mockAccessor.Object);
    }

    [Fact]
    public async Task Invoke_ShouldUseExistingCorrelationId_WhenPresentInHeader()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var headers = new Dictionary<string, string> { { CorrelationIdHeader, correlationId.ToString() } };
        var bindingData = new Dictionary<string, object?>
        {
            { "Headers", JsonSerializer.Serialize(headers) }
        };
        _mockContext.Setup(c => c.BindingContext.BindingData).Returns(bindingData);

        // Act
        await _middleware.Invoke(_mockContext.Object, _mockNext.Object);

        // Assert
        Assert.Equal(_mockContext.Object.Items[CorrelationIdHeader], correlationId.ToString());

        var logEvents = InMemorySink.Instance.LogEvents;
        var logMessage = logEvents.FirstOrDefault()?.RenderMessage();
        Assert.Contains($"Using existing correlation ID: \"{correlationId}\"", logMessage);

        _mockNext.Verify(n => n(_mockContext.Object), Times.Once);
    }

    [Fact]
    public async Task Invoke_ShouldCreateCorrelationId_WhenCorrelationIdHeaderMissing()
    {
        // Arrange
        var headers = new Dictionary<string, string>();
        var bindingData = new Dictionary<string, object?>
        {
            { "Headers", JsonSerializer.Serialize(headers) }
        };
        _mockContext.Setup(c => c.BindingContext.BindingData).Returns(bindingData);

        // Act
        await _middleware.Invoke(_mockContext.Object, _mockNext.Object);

        // Assert
        var correlationId = _mockContext.Object.Items[CorrelationIdHeader]?.ToString();
        Assert.True(Guid.TryParse(correlationId, out _));

        var logEvents = InMemorySink.Instance.LogEvents;
        var logMessage = logEvents.FirstOrDefault()?.RenderMessage();
        Assert.Contains($"Generated new correlation ID: \"{correlationId}\"", logMessage);

        _mockNext.Verify(n => n(_mockContext.Object), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Invoke_ShouldCreateCorrelationId_WhenCorrelationIdHeaderValueEmpty(string? headerValue)
    {
        // Arrange
        var headers = new Dictionary<string, string?> { { CorrelationIdHeader, headerValue } };
        var bindingData = new Dictionary<string, object?>
        {
            { "Headers", JsonSerializer.Serialize(headers) }
        };
        _mockContext.Setup(c => c.BindingContext.BindingData).Returns(bindingData);

        // Act
        await _middleware.Invoke(_mockContext.Object, _mockNext.Object);

        // Assert
        var correlationId = _mockContext.Object.Items[CorrelationIdHeader]?.ToString();
        Assert.True(Guid.TryParse(correlationId, out _));

        var logEvents = InMemorySink.Instance.LogEvents;
        var logMessage = logEvents.FirstOrDefault()?.RenderMessage();
        Assert.Contains($"Generated new correlation ID: \"{correlationId}\"", logMessage);

        _mockNext.Verify(n => n(_mockContext.Object), Times.Once);
    }

    [Fact]
    public async Task Invoke_ShouldCreateCorrelationId_WhenNoHeadersPresent()
    {
        // Arrange
        var bindingData = new Dictionary<string, object?>();
        _mockContext.Setup(c => c.BindingContext.BindingData).Returns(bindingData);

        // Act
        await _middleware.Invoke(_mockContext.Object, _mockNext.Object);

        // Assert
        var correlationId = _mockContext.Object.Items[CorrelationIdHeader]?.ToString();
        Assert.True(Guid.TryParse(correlationId, out _));

        var logEvents = InMemorySink.Instance.LogEvents;
        var logMessage = logEvents.FirstOrDefault()?.RenderMessage();
        Assert.Contains($"Generated correlation ID as headers were missing: \"{correlationId}\"", logMessage);

        _mockNext.Verify(n => n(_mockContext.Object), Times.Once);
    }
}
