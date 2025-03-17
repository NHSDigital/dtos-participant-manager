using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ParticipantManager.Experience.API.Services;
using ParticipantManager.Shared.Client;

namespace ParticipantManager.Experience.API.Functions;

public class GetParticipantIdFunction(
  ILogger<GetParticipantIdFunction> logger,
  ICrudApiClient crudApiClient,
  ITokenService tokenService,
  IFeatureFlagClient featureFlagClient)
{
  [Function("GetParticipantId")]
  public async Task<IActionResult> GetParticipantId(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "participant")]
    HttpRequestData req)
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

      var participant = await crudApiClient.GetParticipantByNhsNumberAsync(nhsNumber);
      if (participant == null)
      {
        logger.LogError("Failed to find participant for NhsNumber: {@NhsNumber}",
          new { NhsNumber = nhsNumber });
        return new NotFoundObjectResult("Unable to find participant");
      }

      var enabled = await featureFlagClient.IsFeatureEnabled("mays_mvp");

      if (!enabled)
      {
        return new ForbidResult();
      }

      logger.LogInformation("Found participant for NhsNumber: {@NhsNumber}",
        new { NhsNumber = nhsNumber });
      return new OkObjectResult(participant.ParticipantId);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Invalid: Bad Request");
      return new BadRequestObjectResult(ex.Message);
    }
  }
}
