namespace ParticipantManager.Experience.API.Functions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ParticipantManager.Experience.API.Client;
using Microsoft.AspNetCore.Http;
using System.Runtime.CompilerServices;
using ParticipantManager.Experience.API.Services;

public class PathwayAssignmentFunctions(ILogger<PathwayAssignmentFunctions> logger, ICrudApiClient crudApiClient, ITokenService tokenService)
{
  [Function("GetPathwayAssignmentById")]
  public async Task<IActionResult> GetParticipantById([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pathwayassignments/{assignmentid}")] HttpRequestData req)
  {
    try {
    logger.LogDebug("{PathwayAssignmentsDetails} Function Called", nameof(PathwayAssignmentFunctions));
    // Extract the Authorization Header

   if (!req.Headers.TryGetValues("Authorization", out var authHeaderValues))
    {
      return new UnauthorizedResult();
    }

    var token = tokenService.ExtractToken(authHeaderValues);
    if (token == null || tokenService.ValidateToken(token) == null)
    {
      return new UnauthorizedResult();
    }
      logger.LogInformation("Token: {Token}", token);

      // logger.LogInformation("Extracted NHS Number: {NhsNumber}", nhsNumber);
      // var pathwayAssignments = await crudApiClient.GetPathwayAssignmentsAsync(nhsNumber);
      // if (pathwayAssignments == null)
      // {
      //   throw new Exception("Unable to find pathway assignments");
      // }

      // return new OkObjectResult(pathwayAssignments);
      return new OkObjectResult(token);
    }
    catch (Exception ex)
    {
      return new BadRequestObjectResult(ex.Message);
    }
  }

  private ObjectResult HandleResponseError(HttpResponseMessage? response, [CallerMemberName] string functionName = "")
  {
    var statusCode = (int?)response?.StatusCode ?? StatusCodes.Status500InternalServerError;
    logger.LogError("GetPathwayAssignments failed. {ErrorMessage}", response?.Content);

    return new ObjectResult(new
    {
      Title = functionName,
      Status = response?.StatusCode,
      Message = $"Failed to retrieve pathway assignments. Status: {response?.StatusCode.ToString() ?? "Null response"}",
    })
    {
      StatusCode = statusCode
    };
  }
}
