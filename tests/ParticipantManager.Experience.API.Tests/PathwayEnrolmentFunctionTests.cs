namespace ParticipantManager.Experience.API.Tests;

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

public class PathwayEnrolmentFunctionTests
{
  private readonly Mock<ICrudApiClient> _crudApiClient = new();
  private readonly PathwayEnrolmentFunction _function;
  private readonly Mock<ILogger<PathwayEnrolmentFunction>> _loggerMock;
  private readonly Mock<ITokenService> _mockTokenService = new();
  private readonly Mock<IFeatureFlagClient> _mockFeatureFlagClient = new();
  private readonly Guid _participantId = Guid.NewGuid();
  private readonly Guid _enrolmentId = Guid.NewGuid();
  private readonly HttpRequestData _request = CreateHttpRequest("");

  public PathwayEnrolmentFunctionTests()
  {
    _loggerMock = new Mock<ILogger<PathwayEnrolmentFunction>>();
    _crudApiClient.Setup(s => s.GetPathwayEnrolmentByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()).Result).Returns(MockPathwayDetails);
    _function = new PathwayEnrolmentFunction(_loggerMock.Object, _crudApiClient.Object, _mockTokenService.Object, _mockFeatureFlagClient.Object);
    _mockFeatureFlagClient.Setup(f => f.IsFeatureEnabledForParticipant(It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync(true);
  }

  [Fact]
  public async Task GetPathwayEnrolmentById_InvalidToken_ReturnsUnauthorized()
  {
    // Arrange
    _mockTokenService.Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>())).ReturnsAsync(AccessTokenResult.Expired());

    // Act
    var response = await _function.GetPathwayEnrolmentById(_request, _participantId, _enrolmentId) as UnauthorizedResult;

    // Assert
    Assert.Equal(StatusCodes.Status401Unauthorized, response?.StatusCode);
  }

  [Fact]
  public async Task GetPathwayEnrolmentById_NoNhsNumber_ReturnsUnauthorized()
  {
    // Arrange
    var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "12345"),
            new(ClaimTypes.Email, "user@example.com"),
            new("custom_claim", "my_value")
        };

    var identity = new ClaimsIdentity(claims, "Bearer");
    var principal = new ClaimsPrincipal(identity);

    _mockTokenService.Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>())).ReturnsAsync(AccessTokenResult.Success(principal));

    // Act
    var response = await _function.GetPathwayEnrolmentById(_request, _participantId, _enrolmentId) as UnauthorizedResult;

    // Assert
    Assert.Equal(StatusCodes.Status401Unauthorized, response?.StatusCode);
  }

  [Fact]
  public async Task GetPathwayEnrolmentById_PathwayEnrolmentIsNull_ReturnsNotFound()
  {
    // Arrange
    var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1234567890"),
            new(ClaimTypes.Email, "user@example.com"),
            new("nhs_number", "1234567890")
        };

    var identity = new ClaimsIdentity(claims, "Bearer");
    var principal = new ClaimsPrincipal(identity);

    _mockTokenService.Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>())).ReturnsAsync(AccessTokenResult.Success(principal));
    _crudApiClient.Setup(s => s.GetPathwayEnrolmentByIdAsync(_participantId, _enrolmentId)).Returns(Task.FromResult<EnrolledPathwayDetailsDto?>(null));

    // Act
    var response = await _function.GetPathwayEnrolmentById(_request, _participantId, _enrolmentId) as NotFoundResult;

    // Assert
    Assert.Equal(StatusCodes.Status404NotFound, response?.StatusCode);
  }

  [Fact]
  public async Task GetPathwayEnrolmentById_NhsNumberDoesNotMatch_ReturnsUnauthorized()
  {
    // Arrange
    var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "12345"),
            new(ClaimTypes.Email, "user@example.com"),
            new("nhs_number", "0987654321") // Different from the mocked data
        };

    var identity = new ClaimsIdentity(claims, "Bearer");
    var principal = new ClaimsPrincipal(identity);

    _mockTokenService.Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>())).ReturnsAsync(AccessTokenResult.Success(principal));

    // Act
    var response = await _function.GetPathwayEnrolmentById(_request, _participantId, _enrolmentId) as UnauthorizedResult;

    // Assert
    Assert.Equal(StatusCodes.Status401Unauthorized, response?.StatusCode);
  }

  [Fact]
  public async Task GetPathwayEnrolmentById_FeatureToggleIsDisabled_ReturnsForbidden()
  {
    // Arrange
    var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1234567890"),
            new(ClaimTypes.Email, "user@example.com"),
            new("nhs_number", "1234567890")
        };

    var identity = new ClaimsIdentity(claims, "Bearer");
    var principal = new ClaimsPrincipal(identity);

    _mockTokenService.Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>())).ReturnsAsync(AccessTokenResult.Success(principal));
    _mockFeatureFlagClient.Setup(f => f.IsFeatureEnabledForParticipant("mays_mvp", _participantId)).ReturnsAsync(false);

    // Act
    var response = await _function.GetPathwayEnrolmentById(_request, _participantId, _enrolmentId);

    // Assert
    Assert.NotNull(response);
    Assert.IsType<ForbidResult>(response);
  }

  [Fact]
  public async Task GetPathwayEnrolmentById_ValidToken_ReturnsOk()
  {
    // Arrange
    var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1234567890"),
            new(ClaimTypes.Email, "user@example.com"),
            new("nhs_number", "1234567890")
        };
    var identity = new ClaimsIdentity(claims, "Bearer");
    var principal = new ClaimsPrincipal(identity);

    _mockTokenService.Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>())).ReturnsAsync(AccessTokenResult.Success(principal));

    // Act
    var response = await _function.GetPathwayEnrolmentById(_request, _participantId, _enrolmentId) as OkObjectResult;

    // Assert
    Assert.Equal(StatusCodes.Status200OK, response?.StatusCode);
    var pathwayEnrolmentDto = response?.Value as EnrolledPathwayDetailsDto;
    Assert.Equal(pathwayEnrolmentDto?.ScreeningName, pathwayEnrolmentDto?.ScreeningName);
  }

  [Fact]
  public async Task GetPathwayEnrolmentById_ExceptionIsThrown_ReturnsBadRequest()
  {
    // Arrange
    var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1234567890"),
            new(ClaimTypes.Email, "user@example.com"),
            new("nhs_number", "1234567890")
        };
    var identity = new ClaimsIdentity(claims, "Bearer");
    var principal = new ClaimsPrincipal(identity);

    _mockTokenService.Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>())).ReturnsAsync(AccessTokenResult.Success(principal));
    _crudApiClient.Setup(s => s.GetPathwayEnrolmentByIdAsync(_participantId, _enrolmentId)).ThrowsAsync(new Exception("Test exception message"));

    // Act
    var response = await _function.GetPathwayEnrolmentById(_request, _participantId, _enrolmentId) as BadRequestObjectResult;

    // Assert
    Assert.Equal(StatusCodes.Status400BadRequest, response?.StatusCode);
    Assert.Equal("Test exception message", response?.Value);
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
      EnrolmentId = Guid.NewGuid(),
      ScreeningName = "Breast Screening",
      Status = "Active",
      EnrolmentDate = DateTime.Now,
      PathwayTypeName = "Breast Screening Regular",
      NextActionDate = DateTime.Now,
      Participant = new ParticipantDto
      {
        NhsNumber = "1234567890"
      }
    };
  }
}
