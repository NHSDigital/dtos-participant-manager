using ParticipantManager.Experience.API.Services;

namespace ParticipantManager.Experience.API.Functions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ParticipantManager.Experience.API.Client;

public class ScreeningEligibilityFunction(ILogger<ScreeningEligibilityFunction> logger, ICrudApiClient crudApiClient, ITokenService tokenService)
{

  [Function("GetScreeningEligibility")]
  public async Task<IActionResult> GetParticipantEligibility([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "eligibility")] HttpRequestData req)
  {
    logger.LogDebug("GetScreeningEligibility execution started at {Timestamp}", DateTime.UtcNow);
    try
    {
      var result = await tokenService.ValidateToken(req);
      if (result.Status == AccessTokenStatus.Valid)
      {
        logger.LogDebug("Access token is valid, looking for NHS Number");

        var nhsNumber = result.Principal.Claims.FirstOrDefault(c => c.Type == "nhs_number")?.Value;
        if (string.IsNullOrEmpty(nhsNumber))
        {
          logger.LogError("Access token doesn't contain NHS number");
          return new UnauthorizedResult();
        }

        logger.LogDebug("Getting Assignments for NhsNumber: {@NhsNumber}", new {NhsNumber = nhsNumber});

        var pathwayAssignments = await crudApiClient.GetPathwayAssignmentsAsync(nhsNumber);
        if (pathwayAssignments == null)
        {
          logger.LogError("Failed to find assignments for Assignments for NhsNumber: {@NhsNumber}", new {NhsNumber = nhsNumber});
          return new NotFoundObjectResult("Unable to find pathway assignments");
        }
        logger.LogInformation("Found pathway assignment for Assignments for NhsNumber: {@NhsNumber}", new {NhsNumber = nhsNumber});
        return new OkObjectResult(pathwayAssignments);
      }

      logger.LogError ("Invalid access token");
      return new UnauthorizedResult();
    }
    catch (Exception ex)
    {
      logger.LogError (ex, "Invalid: Bad Request");
      return new BadRequestObjectResult(ex.Message);
    }
  }
}
