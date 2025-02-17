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
  private readonly ScreeningEligibilityFunction _function;

  public ScreeningEligibilityFunctionTests()
  {
    _loggerMock = new Mock<ILogger<ScreeningEligibilityFunction>>();
    _crudApiClient.Setup(s => s.GetPathwayAssignmentsAsync(It.IsAny<string>()).Result).Returns(MockHttpMessage);
    _function = new ScreeningEligibilityFunction(_loggerMock.Object, _crudApiClient.Object);
  }

  [Fact]
  public async Task GetScreeningEligibility_ShouldReturnUnauthorized_IfNoAuthorizationHeader()
  {
    // Arrange
    var request = CreateHttpRequest(null);

    // Act
    var response = await _function.GetParticipantEligibility(request) as UnauthorizedResult;

    // Assert
    Assert.Equal(StatusCodes.Status401Unauthorized, response?.StatusCode);
  }

  [Fact]
  public async Task GetScreeningEligibility_ShouldReturnBadRequest_IfInvalidToken()
  {
    // Arrange
    var request = CreateHttpRequest("Bearer invalid_token");

    // Act
    var response = await _function.GetParticipantEligibility(request) as BadRequestObjectResult;

    // Assert
    Assert.Equal(StatusCodes.Status400BadRequest, response?.StatusCode);
  }

  [Fact]
  public async Task GetScreeningEligibility_ShouldReturnUnauthorized_IfNoNhsNumber()
  {
    var token = GenerateJwtToken("");
    // Arrange
    var request = CreateHttpRequest($"Bearer {token}");

    // Act
    var response = await _function.GetParticipantEligibility(request) as UnauthorizedResult;

    // Assert
    Assert.Equal(StatusCodes.Status401Unauthorized, response?.StatusCode);
  }

  [Fact]
  public async Task GetScreeningEligibility_ShouldReturnOk_WithValidToken()
  {
    // Arrange
    var token = GenerateJwtToken("1234567890");
    var request = CreateHttpRequest($"Bearer {token}");

    // Act
    var response = await _function.GetParticipantEligibility(request) as OkObjectResult;

    // Assert

    Assert.Equal(StatusCodes.Status200OK, response?.StatusCode);
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
      ;
    request.Setup(r => r.Headers).Returns(headers);
    return request.Object;
  }

  // ✅ Helper Method to Generate a JWT Token
  private static string GenerateJwtToken(string nhsNumber)
  {
    var securityTokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(new[]
        {
                new Claim("nhs_number", nhsNumber)
            }),
      Expires = DateTime.UtcNow.AddMinutes(30)
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateJwtSecurityToken(securityTokenDescriptor);
    return tokenHandler.WriteToken(token);
  }

  // ✅ Mock HTTP Handler to Simulate the CRUD API Response
  private class MockHttpMessageHandler : HttpMessageHandler
  {
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      var response = new HttpResponseMessage(HttpStatusCode.OK)
      {
        Content = new StringContent(JsonSerializer.Serialize(new
        {
          nhsNumber = "1234567890",
          pathways = new[]
              {
                        new { name = "Breast Screening", status = "Eligible" },
                        new { name = "Bowel Screening", status = "Due" }
                    }
        }))
      };
      response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
      return Task.FromResult(response);
    }
  }

  private HttpResponseMessage MockHttpMessage()
  {
    var response = new HttpResponseMessage(HttpStatusCode.OK)
    {
      Content = new StringContent(JsonSerializer.Serialize(new
      {
        nhsNumber = "1234567890",
        pathways = new[]
      {
                        new { name = "Breast Screening", status = "Eligible" },
                        new { name = "Bowel Screening", status = "Due" }
                    }
      }))
    };
    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

    return response;
  }
}
