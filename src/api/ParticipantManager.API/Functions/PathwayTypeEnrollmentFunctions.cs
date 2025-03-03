using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParticipantManager.API.Data;

namespace ParticipantManager.API.Functions;

public class PathwayTypeEnrollmentFunctions
{
  private readonly ParticipantManagerDbContext _dbContext;
  private readonly ILogger<PathwayTypeEnrollmentFunctions> _logger;

  public PathwayTypeEnrollmentFunctions(ILogger<PathwayTypeEnrollmentFunctions> logger,
    ParticipantManagerDbContext dbContext)
  {
    _logger = logger;
    _dbContext = dbContext;
  }

  [Function("PathwayTypeEnrollmentFunctions")]
  public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "participants/pathwaytypeenrollments")]
    HttpRequest req)
  {
    var nhsNumber = req.Query["nhsnumber"].ToString();

    if (string.IsNullOrEmpty(nhsNumber)) return new BadRequestObjectResult("Missing NHS Number");

    var pathwayTypeEnrollments = await _dbContext.PathwayTypeEnrollments
      .Where(p => p.Participant.NHSNumber == nhsNumber)
      .ToListAsync();

    if (pathwayTypeEnrollments == null) return new NotFoundObjectResult("Did not find any enrollments");

    return new OkObjectResult(pathwayTypeEnrollments);
  }

  [Function("GetPathwayEnrollmentById")]
  public async Task<IActionResult> GetPathwayEnrollmentById(
    [HttpTrigger(AuthorizationLevel.Function, "get",
      Route = "participants/pathwaytypeenrollments/nhsnumber/{nhsNumber}/enrollmentid/{enrollmentId:guid}")]
    HttpRequestData req, string nhsNumber, Guid enrollmentId)
  {
    _logger.LogInformation("C# HTTP trigger function GetPathwayEnrollmentById processed a request.");

    if (string.IsNullOrEmpty(nhsNumber)) return new BadRequestObjectResult("Missing NHS Number");

    var pathwayTypeEnrollments = await _dbContext.PathwayTypeEnrollments
      .Where(p => p.Participant.NHSNumber == nhsNumber && p.EnrollmentId == enrollmentId)
      .FirstOrDefaultAsync();

    if (pathwayTypeEnrollments == null) return new NotFoundObjectResult("Did not find any enrollments");

    return new OkObjectResult(pathwayTypeEnrollments);
  }
}
