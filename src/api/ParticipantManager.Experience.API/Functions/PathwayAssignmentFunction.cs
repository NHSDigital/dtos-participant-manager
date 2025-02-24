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
      logger.LogInformation("Processing request: {@TokenName}", result.Principal.Identity.Name);

      var nhsNumber = result.Principal.Claims.FirstOrDefault(c => c.Type == "nhs_number")?.Value;
      logger.LogInformation("NHS number: {@nhsNumber}", nhsNumber);
      if (string.IsNullOrEmpty(nhsNumber))
      {
        logger.LogError("Access token doesn't contain NHS number");
        return new UnauthorizedResult();
      }

      var pathwayAssignment = await crudApiClient.GetPathwayAssignmentByIdAsync(nhsNumber, assignmentId);
      if (pathwayAssignment == null)
      {
        return new NotFoundResult();
      }

      return new OkObjectResult(pathwayAssignment);
    }
    return new UnauthorizedResult();

  }
}
