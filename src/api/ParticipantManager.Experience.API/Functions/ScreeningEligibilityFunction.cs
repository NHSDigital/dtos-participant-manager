using Microsoft.AspNetCore.Mvc;

namespace ParticipantManager.Experience.API.Functions;

using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

public class ScreeningEligibilityFunction(
  IHttpClientFactory httpClientFactory,
  ILogger<ScreeningEligibilityFunction> logger)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    [Function("GetScreeningEligibility")]
    public async Task<IActionResult> GetParticipantEligibility([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        // Extract the Authorization Header
        if (!req.Headers.TryGetValues("Authorization", out var authHeaderValues))
        {
            return new UnauthorizedResult();
        }

        var accessToken = authHeaderValues.FirstOrDefault()?.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(accessToken))
        {
            return new UnauthorizedResult();
        }

        // Decode the JWT to Extract NHS Number
        try
        {

          var handler = new JwtSecurityTokenHandler();
          var token = handler.ReadJwtToken(accessToken);
          var nhsNumber = token.Claims.FirstOrDefault(c => c.Type == "nhs_number")?.Value;

          if (string.IsNullOrEmpty(nhsNumber))
          {
            return new UnauthorizedResult();
          }

          logger.LogInformation($"Extracted NHS Number: {nhsNumber}");

          // Call the Internal CRUD API
          var crudApiUrl =
            $"{Environment.GetEnvironmentVariable("CRUD_API_URL")}/participants/pathwaytypeassignments?nhsnumber={nhsNumber}";

          var requestMessage = new HttpRequestMessage(HttpMethod.Get, crudApiUrl);
          requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

          var response = await _httpClient.SendAsync(requestMessage);

          if (!response.IsSuccessStatusCode)
          {
            return new UnprocessableEntityResult();
          }

          var content = await response.Content.ReadAsStringAsync();

          return new OkObjectResult(content);
        }
        catch (Exception ex)
        {
          return new BadRequestObjectResult(ex.Message);
        }
    }
}
