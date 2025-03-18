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

public class PathwayEnrolmentFunctionTests
{
  private readonly Mock<ICrudApiClient> _crudApiClient = new();
  private readonly PathwayEnrolmentFunction _function;
  private readonly Mock<ILogger<PathwayEnrolmentFunction>> _loggerMock;
  private readonly Mock<ITokenService> _mockTokenService = new();
  private readonly Mock<IFeatureFlagClient> _mockFeatureFlagClient = new();

  public PathwayEnrolmentFunctionTests()
  {
    _loggerMock = new Mock<ILogger<PathwayEnrolmentFunction>>();
    _crudApiClient.Setup(s => s.GetPathwayEnrolmentByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()).Result)
      .Returns(MockPathwayDetails);
    _function = new PathwayEnrolmentFunction(_loggerMock.Object, _crudApiClient.Object, _mockTokenService.Object, _mockFeatureFlagClient.Object);
  }

  [Fact]
  public async Task GetPathwayEnrolmentById_ShouldReturnUnauthorized_IfInvalidToken()
  {
    _mockTokenService
      .Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>()))
      .ReturnsAsync(AccessTokenResult.Expired()); // ✅ Return a valid result

    var request = CreateHttpRequest("");

    // Act
    var response = await _function.GetPathwayEnrolmentById(request, Guid.NewGuid(), Guid.NewGuid()) as UnauthorizedResult;

    // Assert
    Assert.Equal(StatusCodes.Status401Unauthorized, response?.StatusCode);
  }

  [Fact]
  public async Task GetPathwayEnrolmentById_ShouldReturnUnauthorized_IfNoNhsNumber()
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
    var response = await _function.GetPathwayEnrolmentById(request, Guid.NewGuid(), Guid.NewGuid()) as UnauthorizedResult;

    // Assert
    Assert.Equal(StatusCodes.Status401Unauthorized, response?.StatusCode);
  }

  [Fact]
  public async Task GetPathwayEnrolmentById_ShouldReturnOk_WithValidToken()
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

    _mockFeatureFlagClient.Setup(f => f.IsFeatureEnabledForParticipant(It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync(true);

    var request = CreateHttpRequest("");

    // Act
    var response = await _function.GetPathwayEnrolmentById(request, Guid.NewGuid(), Guid.NewGuid()) as OkObjectResult;

    // Assert
    var pathwayEnrolmentDto = (EnrolledPathwayDetailsDto)response.Value;
    Assert.Equal(StatusCodes.Status200OK, response?.StatusCode);
    Assert.Equal(pathwayEnrolmentDto.ScreeningName, MockPathwayDetails().ScreeningName);
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

  private EnrolledPathwayDetailsDto MockPathwayDetails()
  {
    return new EnrolledPathwayDetailsDto
    {
      EnrolmentId = new Guid(),
      ScreeningName = "Breast Screening",
      Status = "Active",
      EnrolmentDate = DateTime.Now,
      PathwayTypeName = "Breast Screening Regular",
      NextActionDate = DateTime.Now,
      Participant = new ParticipantDto {
        NhsNumber = "12345678"
      }
    };
  }
}
