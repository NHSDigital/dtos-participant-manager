namespace ParticipantManager.Experience.API.Functions;

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ParticipantManager.Experience.API.Client;
using ParticipantManager.Experience.API.Services;

public class PathwayAssignmentFunction(ILogger<PathwayAssignmentFunction> logger, ICrudApiClient crudApiClient, ITokenService tokenService)
{
  [Function("GetPathwayAssignmentById")]
  public async Task<IActionResult> GetPathwayAssignmentById([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pathwayassignments/{assignmentid}")] HttpRequestData req, string assignmentId)
  {
    var stopwatch = Stopwatch.StartNew();
    logger.LogInformation("GetPathwayAssignmentById execution started at {Timestamp}", DateTime.UtcNow);
    var result = await tokenService.ValidateToken(req);
    logger.LogInformation("Token validation result: {tokenResult}", result.ToString());

    if (result.Status == AccessTokenStatus.Valid)
    {
      logger.LogInformation("Access token is valid, looking for NHS Number");

      var nhsNumber = result.Principal.Claims.FirstOrDefault(c => c.Type == "nhs_number")?.Value;
      if (string.IsNullOrEmpty(nhsNumber))
      {
        logger.LogError("Access token doesn't contain NHS number");
        return new UnauthorizedResult();
      }
      logger.LogInformation("Getting Assignments for Request {@Request}", new {NhsNumber = nhsNumber, AssignmentId = assignmentId});

      var pathwayAssignment = await crudApiClient.GetPathwayAssignmentByIdAsync(nhsNumber, assignmentId);
      if (pathwayAssignment == null)
      {
        logger.LogError("Failed to find assignments for Assignments for Request {@Request}", new {NhsNumber = nhsNumber, AssignmentId = assignmentId});
        return new NotFoundResult();
      }
      logger.LogInformation("Found pathway assignment for Assignments for Request {@Request}", new {NhsNumber = nhsNumber, AssignmentId = assignmentId});
      return new OkObjectResult(pathwayAssignment);
    }

    logger.LogError ("Invalid access token");
    return new UnauthorizedResult();

  }
}
