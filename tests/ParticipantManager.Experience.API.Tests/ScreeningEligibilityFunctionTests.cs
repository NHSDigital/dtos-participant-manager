using ParticipantManager.Experience.API.DTOs;
using ParticipantManager.Experience.API.Services;

namespace ParticipantManager.Experience.API.Tests;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.IdentityModel.Tokens;
using ParticipantManager.Experience.API.Functions;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ParticipantManager.Experience.API.Client;

public class ScreeningEligibilityFunctionTests
{
  private readonly Mock<ILogger<ScreeningEligibilityFunction>> _loggerMock;
  private readonly Mock<ICrudApiClient> _crudApiClient = new();
  private readonly Mock<ITokenService> _mockTokenService = new();
  private readonly ScreeningEligibilityFunction _function;

  public ScreeningEligibilityFunctionTests()
  {
    _loggerMock = new Mock<ILogger<ScreeningEligibilityFunction>>();
    _crudApiClient.Setup(s => s.GetPathwayAssignmentsAsync(It.IsAny<string>()).Result).Returns(MockListPathwayAssignments);
    _function = new ScreeningEligibilityFunction(_loggerMock.Object, _crudApiClient.Object, _mockTokenService.Object);
  }

  [Fact]
  public async Task GetScreeningEligibility_ShouldReturnUnauthorized_IfInvalidToken()
  {
    _mockTokenService
      .Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>())).ReturnsAsync(AccessTokenResult.Expired()); // ✅ Return a valid result

    var request = CreateHttpRequest($"");

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
      new Claim(ClaimTypes.NameIdentifier, "12345"),
      new Claim(ClaimTypes.Email, "user@example.com"),
      new Claim("custom_claim", "my_value")
    };
    var identity = new ClaimsIdentity(claims, "Bearer");
    var principal = new ClaimsPrincipal(identity);

    _mockTokenService
      .Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>())).ReturnsAsync(AccessTokenResult.Success(principal)); // ✅ Return a valid result

    var request = CreateHttpRequest($"");

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
      new Claim(ClaimTypes.NameIdentifier, "12345678"),
      new Claim(ClaimTypes.Email, "user@example.com"),
      new Claim("nhs_number", "12345678")
    };
    var identity = new ClaimsIdentity(claims, "Bearer");
    var principal = new ClaimsPrincipal(identity);

    _mockTokenService
      .Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>())).ReturnsAsync(AccessTokenResult.Success(principal)); // ✅ Return a valid result

    var request = CreateHttpRequest("");

    // Act
    var response = await _function.GetParticipantEligibility(request) as OkObjectResult;

    // Assert

    Assert.Equal(StatusCodes.Status200OK, response?.StatusCode);
    Assert.Equal(2, ((List<PathwayAssignmentDTO>)response?.Value).Count);
  }

  // ✅ Helper Method to Create Mock HTTP Request
  private static HttpRequestData CreateHttpRequest(string? authHeader)
  {
    var context = new Mock<FunctionContext>();
    var request = new Mock<HttpRequestData>(MockBehavior.Strict, context.Object);
    var headers = new HttpHeadersCollection(new List<KeyValuePair<string, string>>());
    if (!string.IsNullOrEmpty(authHeader))
    {
      headers.Add("Authorization", $"{authHeader}");
    }
    request.Setup(r => r.Headers).Returns(headers);
    return request.Object;
  }

  private List<PathwayAssignmentDTO> MockListPathwayAssignments()
  {
    return new List<PathwayAssignmentDTO>()
    {
      new PathwayAssignmentDTO() {
        AssignmentId = "123",
        ScreeningName = "BreastScreening"
        },
      new PathwayAssignmentDTO() {
        AssignmentId = "1234",
        ScreeningName = "BowelScreening"
        }
    };
  }
}
