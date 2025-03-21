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
using ParticipantManager.TestUtils;

public class GetParticipantIdFunctionTests
{
  private readonly Mock<ICrudApiClient> _crudApiClient = new();
  private readonly GetParticipantIdFunction _function;
  private readonly Mock<ILogger<GetParticipantIdFunction>> _loggerMock;
  private readonly Mock<ITokenService> _mockTokenService = new();
  private readonly Mock<IFeatureFlagClient> _mockFeatureFlagClient = new();
  private readonly Guid _validParticipantId = Guid.NewGuid();

  public GetParticipantIdFunctionTests()
  {
    _loggerMock = new Mock<ILogger<GetParticipantIdFunction>>();
    _crudApiClient.Setup(s => s.GetParticipantByNhsNumberAsync(It.IsAny<string>())).ReturnsAsync(MockParticipant);
    _function = new GetParticipantIdFunction(_loggerMock.Object, _crudApiClient.Object, _mockTokenService.Object, _mockFeatureFlagClient.Object);
  }

  [Fact]
  public async Task GetParticipantId_ShouldReturnUnauthorized_IfInvalidToken()
  {
    // Arrange
    _mockTokenService.Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>())).ReturnsAsync(AccessTokenResult.Expired());
    // Call the static method with authentication header
    var request = SetupRequest.CreateHttpRequest("Bearer token");

    // Act
    var response = await _function.GetParticipantId(request) as UnauthorizedResult;

    // Assert
    Assert.Equal(StatusCodes.Status401Unauthorized, response?.StatusCode);
  }

  [Fact]
  public async Task GetParticipantId_NoNhsNumber_ShouldReturnUnauthorized()
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
    var request = SetupRequest.CreateHttpRequest("Bearer token");

    // Act
    var response = await _function.GetParticipantId(request) as UnauthorizedResult;

    // Assert
    Assert.Equal(StatusCodes.Status401Unauthorized, response?.StatusCode);
  }

  [Fact]
  public async Task GetParticipantId_ParticipantIsNull_ReturnsNotFound()
  {
    // Arrange
    var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "12345"),
            new(ClaimTypes.Email, "user@example.com"),
            new("nhs_number", "12345678")
        };
    var identity = new ClaimsIdentity(claims, "Bearer");
    var principal = new ClaimsPrincipal(identity);

    _mockTokenService.Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>())).ReturnsAsync(AccessTokenResult.Success(principal));
    _crudApiClient.Setup(s => s.GetParticipantByNhsNumberAsync("12345678")).Returns(Task.FromResult<ParticipantDto?>(null));

    var request = SetupRequest.CreateHttpRequest("Bearer token");

    // Act
    var response = await _function.GetParticipantId(request) as NotFoundObjectResult;

    // Assert
    Assert.Equal(StatusCodes.Status404NotFound, response?.StatusCode);
    Assert.Equal("Unable to find participant", response?.Value);
  }

  [Fact]
  public async Task GetParticipantId_FeatureToggleDisabled_ReturnsForbidden()
  {
    // Arrange
    var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "12345678"),
            new(ClaimTypes.Email, "user@example.com"),
            new("nhs_number", "12345678")
        };
    var identity = new ClaimsIdentity(claims, "Bearer");
    var principal = new ClaimsPrincipal(identity);

    _mockTokenService.Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>())).ReturnsAsync(AccessTokenResult.Success(principal));
    _mockFeatureFlagClient.Setup(f => f.IsFeatureEnabledForParticipant("mays_mvp", _validParticipantId)).ReturnsAsync(false);
    var request = SetupRequest.CreateHttpRequest("Bearer token");

    // Act
    var response = await _function.GetParticipantId(request);

    // Assert
    Assert.NotNull(response);
    Assert.IsType<ForbidResult>(response);
  }

  [Fact]
  public async Task GetParticipantId_WithValidToken_ReturnsOk()
  {
    // Arrange
    var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "12345678"),
            new(ClaimTypes.Email, "user@example.com"),
            new("nhs_number", "12345678")
        };
    var identity = new ClaimsIdentity(claims, "Bearer");
    var principal = new ClaimsPrincipal(identity);

    _mockTokenService.Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>())).ReturnsAsync(AccessTokenResult.Success(principal));
    _mockFeatureFlagClient.Setup(f => f.IsFeatureEnabledForParticipant("mays_mvp", _validParticipantId)).ReturnsAsync(true);
    var request = SetupRequest.CreateHttpRequest("Bearer token");

    // Act
    var response = await _function.GetParticipantId(request) as OkObjectResult;

    // Assert
    Assert.Equal(StatusCodes.Status200OK, response?.StatusCode);
    Assert.Equal(_validParticipantId, response?.Value);
  }

  [Fact]
  public async Task GetParticipantId_ExceptionThrown_ReturnBadRequest()
  {
    // Arrange
    var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "12345678"),
            new(ClaimTypes.Email, "user@example.com"),
            new("nhs_number", "12345678")
        };
    var identity = new ClaimsIdentity(claims, "Bearer");
    var principal = new ClaimsPrincipal(identity);

    _mockTokenService.Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>())).ReturnsAsync(AccessTokenResult.Success(principal));
    _crudApiClient.Setup(s => s.GetParticipantByNhsNumberAsync("12345678")).ThrowsAsync(new Exception("Test exception message"));
    var request = SetupRequest.CreateHttpRequest("Bearer token");

    // Act
    var response = await _function.GetParticipantId(request) as BadRequestObjectResult;

    // Assert
    Assert.Equal(StatusCodes.Status400BadRequest, response?.StatusCode);
    Assert.Equal("Test exception message", response?.Value);
  }

  private ParticipantDto MockParticipant()
  {
    return new ParticipantDto
    {
      ParticipantId = _validParticipantId,
      Name = "Test User",
      NhsNumber = "12345678"
    };
  }
}
