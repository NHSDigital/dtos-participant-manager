using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ParticipantManager.Experience.API.Client;
using ParticipantManager.Experience.API.Services;

namespace ParticipantManager.Experience.API.Functions;

public class PathwayAssignmentFunction(
  ILogger<PathwayAssignmentFunction> logger,
  ICrudApiClient crudApiClient,
  ITokenService tokenService)
{
  [Function("GetPathwayAssignmentById")]
  public async Task<IActionResult> GetPathwayAssignmentById(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pathwayassignments/{assignmentid}")] HttpRequestData req,
    string assignmentId)
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

      var pathwayAssignment = await crudApiClient.GetPathwayAssignmentByIdAsync(nhsNumber, assignmentId);
      if (pathwayAssignment == null)
      {
        logger.LogError("Failed to find pathway assignment for Request {@Request}",
          new { NhsNumber = nhsNumber, AssignmentId = assignmentId });
        return new NotFoundResult();
      }

      logger.LogInformation("Found pathway assignment for Request {@Request}",
        new { NhsNumber = nhsNumber, AssignmentId = assignmentId });
      return new OkObjectResult(pathwayAssignment);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Invalid: Bad Request");
      return new BadRequestObjectResult(ex.Message);
    }
  }
}
