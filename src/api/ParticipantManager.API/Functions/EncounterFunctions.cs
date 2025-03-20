using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ParticipantManager.API.Functions;

public class EncounterFunctions
{
  private readonly ILogger<EncounterFunctions> _logger;

  public EncounterFunctions(ILogger<EncounterFunctions> logger)
  {
    _logger = logger;
  }

  [Function("EncounterFunctions")]
  public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
  {
    _logger.LogInformation("C# HTTP trigger function processed a request.");
    return new OkObjectResult("Welcome to Azure Functions!");
  }
}
