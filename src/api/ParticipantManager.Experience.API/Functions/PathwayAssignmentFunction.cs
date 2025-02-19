namespace ParticipantManager.Experience.API.Functions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ParticipantManager.Experience.API.Client;
using Microsoft.AspNetCore.Http;
using System.Runtime.CompilerServices;
using ParticipantManager.Experience.API.Services;

public class PathwayAssignmentFunction(ILogger<PathwayAssignmentFunction> logger, ICrudApiClient crudApiClient, ITokenService tokenService)
{
  [Function("GetPathwayAssignmentById")]
  public async Task<IActionResult> GetPathwayAssignmentById([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pathwayassignments/{assignmentid}")] HttpRequestData req)
  {
    var result = await tokenService.ValidateToken(req);

    if (result.Status == AccessTokenStatus.Valid)
    {
      logger.LogInformation($"Request received for {result.Principal.Identity.Name}.");
      return new OkResult();
    }
    return new UnauthorizedResult();

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
