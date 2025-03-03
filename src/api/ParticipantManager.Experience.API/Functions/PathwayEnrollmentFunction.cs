using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ParticipantManager.Experience.API.Services;
using ParticipantManager.Shared.Client;

namespace ParticipantManager.Experience.API.Functions;

public class PathwayEnrollmentFunction(
  ILogger<PathwayEnrollmentFunction> logger,
  ICrudApiClient crudApiClient,
  ITokenService tokenService)
{
  [Function("GetPathwayEnrollmentById")]
  public async Task<IActionResult> GetPathwayEnrollmentById(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pathwayenrollments/{enrollmentid}")]
    HttpRequestData req,
    string enrollmentId)
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

      var pathwayEnrollment = await crudApiClient.GetPathwayEnrollmentByIdAsync(nhsNumber, enrollmentId);
      if (pathwayEnrollment == null)
      {
        logger.LogError("Failed to find pathway enrollment for Request {@Request}",
          new { NhsNumber = nhsNumber, EnrollmentId = enrollmentId });
        return new NotFoundResult();
      }

      logger.LogInformation("Found pathway enrollment for Request {@Request}",
        new { NhsNumber = nhsNumber, EnrollmentId = enrollmentId });
      return new OkObjectResult(pathwayEnrollment);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Invalid: Bad Request");
      return new BadRequestObjectResult(ex.Message);
    }
  }
}
