using ParticipantManager.Experience.API.Services;

namespace ParticipantManager.Experience.API.Functions;

using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ParticipantManager.Experience.API.Client;
using Microsoft.AspNetCore.Http;
using System.Runtime.CompilerServices;
public class ScreeningEligibilityFunction(ILogger<ScreeningEligibilityFunction> logger, ICrudApiClient crudApiClient, ITokenService tokenService)
{

  [Function("GetScreeningEligibility")]
  public async Task<IActionResult> GetParticipantEligibility([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "eligibility")] HttpRequestData req)
  {
    logger.LogDebug("{GetScreeningEligibility} Function Called", nameof(GetParticipantEligibility));
    try
    {
      var result = await tokenService.ValidateToken(req);

      if (result.Status == AccessTokenStatus.Valid)
      {
        var nhsNumber = result.Principal.Claims.FirstOrDefault(c => c.Type == "nhs_number")?.Value;
        if (string.IsNullOrEmpty(nhsNumber))
        {
          return new UnauthorizedResult();
        }

        logger.LogInformation("Extracted NHS Number: {@NhsNumber}", new { NhsNumber = nhsNumber });
        var pathwayAssignments = await crudApiClient.GetPathwayAssignmentsAsync(nhsNumber);
        if (pathwayAssignments == null)
        {
          throw new Exception("Unable to find pathway assignments");
        }

        return new OkObjectResult(pathwayAssignments);
      }

      return new UnauthorizedResult();
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
