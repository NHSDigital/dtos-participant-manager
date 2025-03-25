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

public class ScreeningEligibilityFunctionTests
{
    private readonly Mock<ICrudApiClient> _crudApiClient = new();
    private readonly ScreeningEligibilityFunction _function;
    private readonly Mock<ILogger<ScreeningEligibilityFunction>> _loggerMock;
    private readonly Mock<ITokenService> _mockTokenService = new();
    private readonly Mock<IFeatureFlagClient> _mockFeatureFlagClient = new();
    private readonly HttpRequestData _request = CreateHttpRequest("");
    private readonly Guid _participantId = Guid.NewGuid();

    public ScreeningEligibilityFunctionTests()
    {
        _loggerMock = new Mock<ILogger<ScreeningEligibilityFunction>>();
        _crudApiClient.Setup(s => s.GetPathwayEnrolmentsAsync(It.IsAny<Guid>()).Result)
            .Returns(MockListPathwayEnrolments);
        _function = new ScreeningEligibilityFunction(_loggerMock.Object, _crudApiClient.Object,
            _mockTokenService.Object,
            _mockFeatureFlagClient.Object);
        _mockFeatureFlagClient.Setup(f => f.IsFeatureEnabledForParticipant("mays_mvp", _participantId))
            .ReturnsAsync(true);
    }

    [Fact]
    public async Task GetScreeningEligibility_InvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        _mockTokenService.Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>()))
            .ReturnsAsync(AccessTokenResult.Expired());

        // Act
        var response = await _function.GetScreeningEligibility(_request, Guid.NewGuid()) as UnauthorizedResult;

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, response?.StatusCode);
    }

    [Fact]
    public async Task GetScreeningEligibility_NoNhsNumber_ReturnsUnauthorized()
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

        _mockTokenService.Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>()))
            .ReturnsAsync(AccessTokenResult.Success(principal));

        // Act
        var response = await _function.GetScreeningEligibility(_request, Guid.NewGuid()) as UnauthorizedResult;

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, response?.StatusCode);
    }

    [Fact]
    public async Task GetScreeningEligibility_ParticipantIdIsEmpty_ReturnsUnauthorized()
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

        _mockTokenService.Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>()))
            .ReturnsAsync(AccessTokenResult.Success(principal));

        // Act
        var response = await _function.GetScreeningEligibility(_request, Guid.Empty) as UnauthorizedResult;

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, response?.StatusCode);
    }

    [Fact]
    public async Task GetScreeningEligibility_PathwayEnrolmentsReturnsEmptyCollection_ReturnsOk()
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

        _mockTokenService.Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>()))
            .ReturnsAsync(AccessTokenResult.Success(principal));
        _crudApiClient.Setup(s => s.GetPathwayEnrolmentsAsync(_participantId)).ReturnsAsync([]);

        // Act
        var response = await _function.GetScreeningEligibility(_request, _participantId) as OkObjectResult;

        // Assert
        Assert.IsType<OkObjectResult>(response);
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetScreeningEligibility_NhsNumberDoesNotMatch_ReturnsUnauthorized()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "12345"),
            new(ClaimTypes.Email, "user@example.com"),
            new("nhs_number", "87654321") // Different NHS number than in mock enrolments
        };

        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        _mockTokenService.Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>()))
            .ReturnsAsync(AccessTokenResult.Success(principal));

        // Act
        var response = await _function.GetScreeningEligibility(_request, _participantId) as UnauthorizedResult;

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, response?.StatusCode);
    }

    [Fact]
    public async Task GetScreeningEligibility_FeatureIsDisabled_ReturnsForbidden()
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

        _mockTokenService.Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>()))
            .ReturnsAsync(AccessTokenResult.Success(principal));
        _mockFeatureFlagClient.Setup(f => f.IsFeatureEnabledForParticipant("mays_mvp", _participantId))
            .ReturnsAsync(false);

        // Act
        var response = await _function.GetScreeningEligibility(_request, _participantId);

        // Assert
        Assert.NotNull(response);
        Assert.IsType<ForbidResult>(response);
    }

    [Fact]
    public async Task GetScreeningEligibility_ValidToken_ReturnsOk()
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

        _mockTokenService.Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>()))
            .ReturnsAsync(AccessTokenResult.Success(principal));

        // Act
        var response = await _function.GetScreeningEligibility(_request, _participantId) as OkObjectResult;

        // Assert
        Assert.Equal(StatusCodes.Status200OK, response?.StatusCode);
        var enrollments = response?.Value as List<PathwayEnrolmentDto>;
        Assert.NotNull(enrollments);
        Assert.Equal(2, enrollments.Count);
    }

    [Fact]
    public async Task GetScreeningEligibility_ExceptionIsThrown_ReturnsBadRequest()
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

        _mockTokenService.Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>()))
            .ReturnsAsync(AccessTokenResult.Success(principal));
        _crudApiClient.Setup(s => s.GetPathwayEnrolmentsAsync(_participantId))
            .ThrowsAsync(new Exception("Test exception message"));

        // Act
        var response = await _function.GetScreeningEligibility(_request, _participantId) as BadRequestObjectResult;

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, response?.StatusCode);
        Assert.Equal("Test exception message", response?.Value);
    }

    private static HttpRequestData CreateHttpRequest(string? authHeader)
    {
        var context = new Mock<FunctionContext>();
        var request = new Mock<HttpRequestData>(MockBehavior.Strict, context.Object);
        var headers = new HttpHeadersCollection(new List<KeyValuePair<string, string>>());
        if (!string.IsNullOrEmpty(authHeader)) headers.Add("Authorization", $"{authHeader}");
        request.Setup(r => r.Headers).Returns(headers);
        return request.Object;
    }

    private List<PathwayEnrolmentDto> MockListPathwayEnrolments()
    {
        return new List<PathwayEnrolmentDto>
        {
            new()
            {
                EnrolmentId = "123",
                ScreeningName = "BreastScreening",
                Participant = new ParticipantDto
                {
                    NhsNumber = "12345678",
                }
            },
            new()
            {
                EnrolmentId = "1234",
                ScreeningName = "BowelScreening",
                Participant = new ParticipantDto
                {
                    NhsNumber = "12345678",
                }
            }
        };
    }
}
