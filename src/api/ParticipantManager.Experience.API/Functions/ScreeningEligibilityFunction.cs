namespace ParticipantManager.Experience.API.Functions;

using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ParticipantManager.Experience.API.Client;
using Microsoft.AspNetCore.Http;
using System.Runtime.CompilerServices;
public class ScreeningEligibilityFunction(ILogger<ScreeningEligibilityFunction> logger, ICrudApiClient crudApiClient)
{

  [Function("GetScreeningEligibility")]
  public async Task<IActionResult> GetParticipantEligibility([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "eligibility")] HttpRequestData req)
  {
    logger.LogDebug("{GetScreeningEligibility} Function Called", nameof(GetParticipantEligibility));
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

      logger.LogInformation("Extracted NHS Number: {NhsNumber}", nhsNumber);

      var response = await crudApiClient.GetPathwayAssignmentsAsync(nhsNumber);
      if (response == null || !response.IsSuccessStatusCode)
      {
        return HandleResponseError(response);
      }

      var content = await response.Content.ReadAsStringAsync();
      return new OkObjectResult(content);
    }
    catch (Exception ex)
    {
      return new BadRequestObjectResult(ex.Message);
    }
  }


  private ObjectResult HandleResponseError(HttpResponseMessage? response, [CallerMemberName] string functionName = "")
  {
    var statusCode = (int?)response?.StatusCode ?? StatusCodes.Status500InternalServerError;
    logger.LogError("GetPathwayAssignments failed. {ErrorMessage}", response?.Content);

    return new ObjectResult(new
    {
      Title = functionName,
      Status = response?.StatusCode,
      Message = $"Failed to retrieve pathway assignments. Status: {response?.StatusCode.ToString() ?? "Null response"}",
    })
    {
      StatusCode = statusCode
    };
  }
}
