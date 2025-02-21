namespace ErrorHandlerService.Tests;

using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using ParticipantManager.API.Models;
using Xunit;
using System;
using System.Net.Http;
using Shared.ErrorHandling;

public class ErrorHandlerServiceTests
{
  private readonly Mock<ILogger> _loggerMock = new();
  private readonly ErrorHandlerService _errorHandlerService;

  public ErrorHandlerServiceTests()
  {
    _errorHandlerService = new ErrorHandlerService(_loggerMock.Object);
  }

  [Fact]
  public void HandleResponseError_WhenResponseIsNull_ReturnsDefaultValues()
  {
    // Act
    var result = _errorHandlerService.HandleResponseError(null);
    var response = (ErrorResponse)result.Value;

    // Assert
    Assert.Equal(500, result.StatusCode);
    Assert.Equal("Unknown", response.Status.ToString());
    Assert.Contains("Unknown", response.Message);
    Assert.All(response.Headers.Values, value => Assert.Equal("not-found", value));
  }

  [Fact]
  public void HandleResponseError_WhenResponseHasHeaders_ReturnsHeaderValues()
  {
    // Arrange
    var response = new HttpResponseMessage(HttpStatusCode.NotFound);
    response.Headers.Add("x-correlation-id", "test-correlation");
    response.Headers.Add("x-request-id", "test-request");
    response.Headers.Add("x-session-id", "test-session");
    response.Headers.Add("x-trace-id", "test-trace");

    // Act
    var result = _errorHandlerService.HandleResponseError(response);
    var errorResponse = (ErrorResponse)result.Value;

    // Assert
    Assert.Equal("test-correlation", errorResponse.Headers["x-correlation-id"]);
    Assert.Equal("test-request", errorResponse.Headers["x-request-id"]);
    Assert.Equal("test-session", errorResponse.Headers["x-session-id"]);
    Assert.Equal("test-trace", errorResponse.Headers["x-trace-id"]);
  }

  [Theory]
  [InlineData(HttpStatusCode.NotFound, 404)]
  [InlineData(HttpStatusCode.BadRequest, 400)]
  [InlineData(HttpStatusCode.InternalServerError, 500)]
  public void HandleResponseError_ReturnsCorrectStatusCode(HttpStatusCode statusCode, int expectedStatusCode)
  {
    // Arrange
    var response = new HttpResponseMessage(statusCode);

    // Act
    var result = _errorHandlerService.HandleResponseError(response);
    var objectResult = result;

    // Assert
    Assert.Equal(expectedStatusCode, objectResult.StatusCode);
  }

  [Fact]
  public void HandleResponseError_LogsErrorMessage()
  {
    // Arrange
    var response = new HttpResponseMessage(HttpStatusCode.NotFound);
    response.Headers.Add("x-correlation-id", "test-correlation");

    // Act
    _errorHandlerService.HandleResponseError(response);

    // Assert
    _loggerMock.Verify(x => x.LogError(
        It.Is<string>(s => s.Contains("Operation")),
        It.IsAny<string>(),
        It.Is<string>(s => s == "NotFound"),
        It.IsAny<Dictionary<string, string>>(),
        It.IsAny<string>()
    ), Times.Once);
  }

  [Fact]
  public void HandleResponseError_IncludesCallerMemberName()
  {
    // Arrange
    var response = new HttpResponseMessage(HttpStatusCode.NotFound);

    // Act
    var result = _errorHandlerService.HandleResponseError(response, "TestMethod");
    var errorResponse = (ErrorResponse)result.Value;

    // Assert
    Assert.Equal("TestMethod", errorResponse.Title);
  }

  [Fact]
  public void HandleResponseError_SetsTimestamp()
  {
    // Arrange
    var response = new HttpResponseMessage(HttpStatusCode.NotFound);

    // Act
    var result = _errorHandlerService.HandleResponseError(response);
    var errorResponse = (ErrorResponse)result.Value;

    // Assert
    Assert.True((DateTime.UtcNow - errorResponse.Timestamp).TotalSeconds < 1);
  }

  [Fact]
  public void HandleResponseError_WhenResponseHasReasonPhrase_IncludesInMessage()
  {
    // Arrange
    var response = new HttpResponseMessage(HttpStatusCode.NotFound)
    {
      ReasonPhrase = "Custom Reason"
    };

    // Act
    var result = _errorHandlerService.HandleResponseError(response);
    var errorResponse = (ErrorResponse)result.Value;

    // Assert
    Assert.Contains("Custom Reason", errorResponse.Message);
  }

  [Fact]
  public void HandleResponseError_WhenHeadersAreMissing_UsesNotFound()
  {
    // Arrange
    var response = new HttpResponseMessage(HttpStatusCode.OK);

    // Act
    var result = _errorHandlerService.HandleResponseError(response);
    var errorResponse = (ErrorResponse)result.Value;

    // Assert
    Assert.All(errorResponse.Headers.Values, value => Assert.Equal("not-found", value));
  }
}

