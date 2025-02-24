namespace ParticipantManager.Experience.API.Functions;

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
    var result = await tokenService.ValidateToken(req);

    if (result.Status == AccessTokenStatus.Valid)
    {
      logger.LogInformation($"Request received for {result.Principal.Identity.Name}.");

      var nhsNumber = result.Principal.Claims.FirstOrDefault(c => c.Type == "nhs_number")?.Value;
      if (string.IsNullOrEmpty(nhsNumber))
      {
        return new UnauthorizedResult();
      }

      logger.LogInformation("Extracted NHS Number: {NhsNumber}", nhsNumber);
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
