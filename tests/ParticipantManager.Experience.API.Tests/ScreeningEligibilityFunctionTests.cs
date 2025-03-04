using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using ParticipantManager.Experience.API.Functions;
using ParticipantManager.Experience.API.Services;
using ParticipantManager.Shared.Client;
using ParticipantManager.Shared.DTOs;

namespace ParticipantManager.Experience.API.Tests;

public class ScreeningEligibilityFunctionTests
{
  private readonly Mock<ICrudApiClient> _crudApiClient = new();
  private readonly ScreeningEligibilityFunction _function;
  private readonly Mock<ILogger<ScreeningEligibilityFunction>> _loggerMock;
  private readonly Mock<ITokenService> _mockTokenService = new();

  public ScreeningEligibilityFunctionTests()
  {
    _loggerMock = new Mock<ILogger<ScreeningEligibilityFunction>>();
    _crudApiClient.Setup(s => s.GetPathwayEnrolmentsAsync(It.IsAny<string>()).Result)
      .Returns(MockListPathwayEnrolments);
    _function = new ScreeningEligibilityFunction(_loggerMock.Object, _crudApiClient.Object, _mockTokenService.Object);
  }

  [Fact]
  public async Task GetScreeningEligibility_ShouldReturnUnauthorized_IfInvalidToken()
  {
    _mockTokenService
      .Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>()))
      .ReturnsAsync(AccessTokenResult.Expired()); // ✅ Return a valid result

    var request = CreateHttpRequest("");

    // Act
    var response = await _function.GetParticipantEligibility(request) as UnauthorizedResult;

    // Assert
    Assert.Equal(StatusCodes.Status401Unauthorized, response?.StatusCode);
  }

  [Fact]
  public async Task GetScreeningEligibility_ShouldReturnUnauthorized_IfNoNhsNumber()
  {
    var claims = new List<Claim>
    {
      new(ClaimTypes.NameIdentifier, "12345"),
      new(ClaimTypes.Email, "user@example.com"),
      new("custom_claim", "my_value")
    };
    var identity = new ClaimsIdentity(claims, "Bearer");
    var principal = new ClaimsPrincipal(identity);

    _mockTokenService
      .Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>()))
      .ReturnsAsync(AccessTokenResult.Success(principal)); // ✅ Return a valid result

    var request = CreateHttpRequest("");

    // Act
    var response = await _function.GetParticipantEligibility(request) as UnauthorizedResult;

    // Assert
    Assert.Equal(StatusCodes.Status401Unauthorized, response?.StatusCode);
  }

  [Fact]
  public async Task GetScreeningEligibility_ShouldReturnOk_WithValidToken()
  {
    var claims = new List<Claim>
    {
      new(ClaimTypes.NameIdentifier, "12345678"),
      new(ClaimTypes.Email, "user@example.com"),
      new("nhs_number", "12345678")
    };
    var identity = new ClaimsIdentity(claims, "Bearer");
    var principal = new ClaimsPrincipal(identity);

    _mockTokenService
      .Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>()))
      .ReturnsAsync(AccessTokenResult.Success(principal)); // ✅ Return a valid result

    var request = CreateHttpRequest("");

    // Act
    var response = await _function.GetParticipantEligibility(request) as OkObjectResult;

    // Assert

    Assert.Equal(StatusCodes.Status200OK, response?.StatusCode);
    Assert.Equal(2, ((List<PathwayEnrolmentDTO>)response?.Value).Count);
  }

  // ✅ Helper Method to Create Mock HTTP Request
  private static HttpRequestData CreateHttpRequest(string? authHeader)
  {
    var context = new Mock<FunctionContext>();
    var request = new Mock<HttpRequestData>(MockBehavior.Strict, context.Object);
    var headers = new HttpHeadersCollection(new List<KeyValuePair<string, string>>());
    if (!string.IsNullOrEmpty(authHeader)) headers.Add("Authorization", $"{authHeader}");
    request.Setup(r => r.Headers).Returns(headers);
    return request.Object;
  }

  private List<PathwayEnrolmentDTO> MockListPathwayEnrolments()
  {
    return new List<PathwayEnrolmentDTO>
    {
      new()
      {
        EnrolmentId = "123",
        ScreeningName = "BreastScreening"
      },
      new()
      {
        EnrolmentId = "1234",
        ScreeningName = "BowelScreening"
      }
    };
  }
}
