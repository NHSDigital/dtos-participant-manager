using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ParticipantManager.Experience.API.Client;
using ParticipantManager.Experience.API.Services;

namespace ParticipantManager.Experience.API.Functions;

public class ScreeningEligibilityFunction(
  ILogger<ScreeningEligibilityFunction> logger,
  ICrudApiClient crudApiClient,
  ITokenService tokenService)
{
  [Function("GetScreeningEligibility")]
  public async Task<IActionResult> GetParticipantEligibility(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "eligibility")] HttpRequestData req)
  {
    try
    {
      var result = await tokenService.ValidateToken(req);

      if (result.Status != AccessTokenStatus.Valid)
      {
        logger.LogError("Invalid access token");
        return new UnauthorizedResult();
      }

      logger.LogInformation("Access token is valid, looking for NHS Number");

      var nhsNumber = result.Principal.Claims.FirstOrDefault(c => c.Type == "nhs_number")?.Value;
      if (string.IsNullOrEmpty(nhsNumber))
      {
        logger.LogError("Access token doesn't contain NHS number");
        return new UnauthorizedResult();
      }

      var pathwayEnrollments = await crudApiClient.GetPathwayEnrollmentsAsync(nhsNumber);
      if (pathwayEnrollments == null)
      {
        logger.LogError("Failed to find pathway enrollments for NhsNumber: {@NhsNumber}",
          new { NhsNumber = nhsNumber });
        return new NotFoundObjectResult("Unable to find pathway enrollments");
      }

      logger.LogInformation("Found pathway enrollments for NhsNumber: {@NhsNumber}",
        new { NhsNumber = nhsNumber });
      return new OkObjectResult(pathwayEnrollments);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Invalid: Bad Request");
      return new BadRequestObjectResult(ex.Message);
    }
  }
}
