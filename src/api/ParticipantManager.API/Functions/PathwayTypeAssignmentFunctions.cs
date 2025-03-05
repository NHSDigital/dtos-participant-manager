using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParticipantManager.API.Data;

namespace ParticipantManager.API.Functions;

public class PathwayTypeAssignmentFunctions
{
  private readonly ParticipantManagerDbContext _dbContext;
  private readonly ILogger<PathwayTypeAssignmentFunctions> _logger;

  public PathwayTypeAssignmentFunctions(ILogger<PathwayTypeAssignmentFunctions> logger,
    ParticipantManagerDbContext dbContext)
  {
    _logger = logger;
    _dbContext = dbContext;
  }

  [Function("PathwayTypeAssignmentFunctions")]
  public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "participants/pathwaytypeassignments")]
    HttpRequest req)
  {
    var nhsNumber = req.Query["nhsnumber"].ToString();

    if (string.IsNullOrEmpty(nhsNumber)) return new BadRequestObjectResult("Missing NHS Number");

    var pathwayTypeAssignments = await _dbContext.PathwayTypeAssignments
      .Where(p => p.Participant.NHSNumber == nhsNumber)
      .ToListAsync();

    if (pathwayTypeAssignments == null) return new NotFoundObjectResult("Did not find any assignments");

    return new OkObjectResult(pathwayTypeAssignments);
  }

  [Function("GetPathwayAssignmentById")]
  public async Task<IActionResult> GetPathwayAssignmentById(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get",
      Route = "participants/pathwaytypeassignments/nhsnumber/{nhsNumber}/assignmentid/{assignmentId:guid}")]
    HttpRequestData req, string nhsNumber, Guid assignmentId)
  {
    _logger.LogInformation("C# HTTP trigger function GetPathwayAssignmentById processed a request.");

    if (string.IsNullOrEmpty(nhsNumber)) return new BadRequestObjectResult("Missing NHS Number");

    var pathwayTypeAssignments = await _dbContext.PathwayTypeAssignments
      .Where(p => p.Participant.NHSNumber == nhsNumber && p.AssignmentId == assignmentId)
      .FirstOrDefaultAsync();

    if (pathwayTypeAssignments == null) return new NotFoundObjectResult("Did not find any assignments");

    return new OkObjectResult(pathwayTypeAssignments);
  }
}
