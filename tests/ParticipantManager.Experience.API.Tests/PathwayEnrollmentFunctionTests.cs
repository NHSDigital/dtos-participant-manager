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

public class PathwayEnrollmentFunctionTests
{
  private readonly Mock<ICrudApiClient> _crudApiClient = new();
  private readonly PathwayEnrollmentFunction _function;
  private readonly Mock<ILogger<PathwayEnrollmentFunction>> _loggerMock;
  private readonly Mock<ITokenService> _mockTokenService = new();

  public PathwayEnrollmentFunctionTests()
  {
    _loggerMock = new Mock<ILogger<PathwayEnrollmentFunction>>();
    _crudApiClient.Setup(s => s.GetPathwayEnrollmentByIdAsync(It.IsAny<string>(), It.IsAny<string>()).Result)
      .Returns(MockPathwayDetails);
    _function = new PathwayEnrollmentFunction(_loggerMock.Object, _crudApiClient.Object, _mockTokenService.Object);
  }

  [Fact]
  public async Task GetPathwayEnrollmentById_ShouldReturnUnauthorized_IfInvalidToken()
  {
    _mockTokenService
      .Setup(s => s.ValidateToken(It.IsAny<HttpRequestData>()))
      .ReturnsAsync(AccessTokenResult.Expired()); // ✅ Return a valid result

    var request = CreateHttpRequest("");

    // Act
    var response = await _function.GetPathwayEnrollmentById(request, "123") as UnauthorizedResult;

    // Assert
    Assert.Equal(StatusCodes.Status401Unauthorized, response?.StatusCode);
  }

  [Fact]
  public async Task GetPathwayEnrollmentById_ShouldReturnUnauthorized_IfNoNhsNumber()
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
    var response = await _function.GetPathwayEnrollmentById(request, "123") as UnauthorizedResult;

    // Assert
    Assert.Equal(StatusCodes.Status401Unauthorized, response?.StatusCode);
  }

  [Fact]
  public async Task GetPathwayEnrollmentById_ShouldReturnOk_WithValidToken()
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
    var response = await _function.GetPathwayEnrollmentById(request, "123") as OkObjectResult;

    // Assert
    var pathwayEnrollmentDto = (EnrolledPathwayDetailsDTO)response.Value;
    Assert.Equal(StatusCodes.Status200OK, response?.StatusCode);
    Assert.Equal(pathwayEnrollmentDto.ScreeningName, MockPathwayDetails().ScreeningName);
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

  private EnrolledPathwayDetailsDTO MockPathwayDetails()
  {
    return new EnrolledPathwayDetailsDTO
    {
      EnrollmentId = new Guid(),
      ScreeningName = "Breast Screening",
      Status = "Active",
      EnrollmentDate = DateTime.Now,
      PathwayName = "Breast Screening Regular",
      NextActionDate = DateTime.Now
    };
  }
}
