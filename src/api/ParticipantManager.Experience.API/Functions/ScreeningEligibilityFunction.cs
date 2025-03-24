using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ParticipantManager.Experience.API.Services;
using ParticipantManager.Shared.Client;
using ParticipantManager.Shared.DTOs;

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
    HttpRequestData req, Guid participantId)
  {
    try
    {
      var result = await tokenService.ValidateToken(req);

      if (result.Status != AccessTokenStatus.Valid)
      {
        logger.LogError("Invalid access token");
        return new UnauthorizedResult();
      }

      var nhsNumber = result.Principal?.Claims.FirstOrDefault(c => c.Type == "nhs_number")?.Value;
      if (string.IsNullOrEmpty(nhsNumber))
      {
        logger.LogError("Access token doesn't contain NHS number");
        return new UnauthorizedResult();
      }

      if (participantId == Guid.Empty)
      {
        logger.LogError("Access token doesn't contain ParticipantId");
        return new UnauthorizedResult();
      }

      var pathwayEnrolments = await crudApiClient.GetPathwayEnrolmentsAsync(participantId);
      if (pathwayEnrolments == null || pathwayEnrolments.FirstOrDefault() == null)
      {
        logger.LogError("Failed to find pathway enrolments for Participant: {@ParticipantId}",
          new { ParticipantId = participantId });
        return new NotFoundObjectResult("Unable to find pathway enrolments");
      }

      //Check that logged in user has access to participant
      if (pathwayEnrolments.Any(pathwayEnrolments => pathwayEnrolments.Participant.NhsNumber != nhsNumber.ToString()))
      {
        logger.LogError("Logged in user does not have access to this record: {@ParticipantId}",
          new { ParticipantId = participantId });
        return new UnauthorizedResult();
      }

      var enabled = await featureFlagClient.IsFeatureEnabledForParticipant("mays_mvp", participantId);

      if (!enabled)
      {
        return new ForbidResult();
      }

      logger.LogInformation("Found pathway enrolments for Participant: {@ParticipantId}",
        new { ParticipantId = participantId });
      return new OkObjectResult(pathwayEnrolments);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Invalid: Bad Request");
      return new BadRequestObjectResult(ex.Message);
    }
  }
}
