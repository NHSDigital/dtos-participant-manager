using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParticipantManager.API.Data;
using Microsoft.EntityFrameworkCore;

namespace ParticipantManager.API.Functions;

public class PathwayTypeAssignmentFunctions
{
  private readonly ILogger<PathwayTypeAssignmentFunctions> _logger;
  private readonly ParticipantManagerDbContext _dbContext;

  public PathwayTypeAssignmentFunctions(ILogger<PathwayTypeAssignmentFunctions> logger, ParticipantManagerDbContext dbContext)
  {
    _logger = logger;
    _dbContext = dbContext;
  }

  [Function("PathwayTypeAssignmentFunctions")]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "participants/pathwaytypeassignments")] HttpRequest req)
  {
    _logger.LogInformation("C# HTTP trigger function processed a request.");

    string nhsNumber = req.Query["nhsnumber"].ToString();

    if (string.IsNullOrEmpty(nhsNumber))
    {
      return new BadRequestObjectResult("Missing NHS Number");
    }

    var pathwayTypeAssignments = await _dbContext.PathwayTypeAssignments
      .Where(p => p.Participant.NHSNumber == nhsNumber)
      .ToListAsync();

    if (pathwayTypeAssignments == null)
    {
      return new NotFoundObjectResult("Did not find any assignments");
    }

    return new OkObjectResult(pathwayTypeAssignments);
  }
}
