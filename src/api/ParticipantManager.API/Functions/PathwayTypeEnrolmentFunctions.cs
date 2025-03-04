using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParticipantManager.API.Data;
using ParticipantManager.API.Models;
using ParticipantManager.Shared.DTOs;

namespace ParticipantManager.API.Functions;

public class PathwayTypeEnrolmentFunctions
{
  private readonly ParticipantManagerDbContext _dbContext;
  private readonly ILogger<PathwayTypeEnrolmentFunctions> _logger;

  public PathwayTypeEnrolmentFunctions(ILogger<PathwayTypeEnrolmentFunctions> logger,
    ParticipantManagerDbContext dbContext)
  {
    _logger = logger;
    _dbContext = dbContext;
  }

  [Function("PathwayTypeEnrolmentFunctions")]
  public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "participants/pathwaytypeenrolments")]
    HttpRequest req)
  {
    var nhsNumber = req.Query["nhsnumber"].ToString();

    if (string.IsNullOrEmpty(nhsNumber)) return new BadRequestObjectResult("Missing NHS Number");

    var pathwayTypeEnrolments = await _dbContext.PathwayTypeEnrolments
      .Where(p => p.Participant.NHSNumber == nhsNumber)
      .ToListAsync();

    if (pathwayTypeEnrolments == null) return new NotFoundObjectResult("Did not find any enrolments");

    return new OkObjectResult(pathwayTypeEnrolments);
  }

  [Function("GetPathwayEnrolmentById")]
  public async Task<IActionResult> GetPathwayEnrolmentById(
    [HttpTrigger(AuthorizationLevel.Function, "get",
      Route = "pathwaytypeenrolments/{enrolmentId:guid}")]
    HttpRequestData req, Guid enrolmentId)
  {
    _logger.LogInformation("C# HTTP trigger function GetPathwayEnrolmentById processed a request.");

    var pathwayTypeEnrolments = await _dbContext.PathwayTypeEnrolments
      .Where(p => p.EnrolmentId == enrolmentId)
      .FirstOrDefaultAsync();

    if (pathwayTypeEnrolments == null) return new NotFoundObjectResult("Did not find any enrolments");

    return new OkObjectResult(pathwayTypeEnrolments);
  }

  [Function("CreatePathwayEnrolment")]
  public async Task<IActionResult> CreatePathwayEnrolment([HttpTrigger(AuthorizationLevel.Function, "post", Route = "participants/pathwayenrolment")]
    HttpRequestData req)
  {
    _logger.LogInformation("C# HTTP trigger function CreatePathwayEnrolment processed a request.");

    CreateParticipantEnrolmentDto participantEnrolmentDto = null;

    try
    {
      JsonSerializerOptions options = new()
      {
        ReferenceHandler = ReferenceHandler.Preserve
      };

      participantEnrolmentDto = JsonSerializer.Deserialize<CreateParticipantEnrolmentDto>(req.Body.ToString(), options);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unable to deserialize event data to CreateParticipantEnrolmentDto object.");
    }

    var pathwayTypeEnrolment = new PathwayTypeEnrolment()
    {
      EnrolmentId = Guid.NewGuid(),
      ScreeningName = "",
      PathwayTypeName = participantEnrolmentDto?.PathwayTypeName ?? "",
      Status = ""
    };

    var result = await _dbContext.PathwayTypeEnrolments.AddAsync(pathwayTypeEnrolment);

    return new CreatedResult($"pathwaytypeenrolments/", pathwayTypeEnrolment);
  }
}
