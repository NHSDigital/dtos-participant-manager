using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ParticipantManager.Experience.API.Services;
using ParticipantManager.Shared.Client;

namespace ParticipantManager.Experience.API.Functions;

public class ScreeningEligibilityFunction(
  ILogger<ScreeningEligibilityFunction> logger,
  ICrudApiClient crudApiClient,
  ITokenService tokenService,
  IFeatureFlagClient featureFlagClient)
{
  [Function("GetScreeningEligibility")]
  public async Task<IActionResult> GetParticipantEligibility(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "participant/{participantId}/eligibility")]
    HttpRequestData req, string participantId)
  {
    try
    {
      var result = await tokenService.ValidateToken(req);

      if (result.Status != AccessTokenStatus.Valid)
      {
        logger.LogError("Invalid access token");
        return new UnauthorizedResult();
      }

      if (string.IsNullOrEmpty(participantId.ToString()))
      {
        logger.LogError("Access token doesn't contain ParticipantId");
        return new UnauthorizedResult();
      }

      var pathwayEnrolments = await crudApiClient.GetPathwayEnrolmentsAsync(nhsNumber);
      if (pathwayEnrolments == null)
      {
        logger.LogError("Failed to find pathway enrolments for NhsNumber: {@NhsNumber}",
          new { NhsNumber = nhsNumber });
        return new NotFoundObjectResult("Unable to find pathway enrolments");
      }

      var enabled = await featureFlagClient.IsFeatureEnabledForParticipant("mays_mvp", participantId);

      if (!enabled)
      {
        return new ForbidResult();
      }

      logger.LogInformation("Found pathway enrolments for NhsNumber: {@NhsNumber}",
        new { NhsNumber = nhsNumber });
      return new OkObjectResult(pathwayEnrolments);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Invalid: Bad Request");
      return new BadRequestObjectResult(ex.Message);
    }
  }
}
