using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ParticipantManager.API.Functions;

public class EpisodeFunctions
{
  private readonly ILogger<EpisodeFunctions> _logger;

  public EpisodeFunctions(ILogger<EpisodeFunctions> logger)
  {
    _logger = logger;
  }

  [Function("EpisodeFunctions")]
  public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
  {
    _logger.LogInformation("C# HTTP trigger function processed a request.");
    return new OkObjectResult("Welcome to Azure Functions!");
  }
}
