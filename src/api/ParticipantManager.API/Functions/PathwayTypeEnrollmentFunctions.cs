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
      Route = "pathwaytypeenrollments/{enrollmentId:guid}")]
    HttpRequestData req, Guid enrollmentId)
  {
    _logger.LogInformation("C# HTTP trigger function GetPathwayEnrollmentById processed a request.");

    var pathwayTypeEnrollments = await _dbContext.PathwayTypeEnrollments
      .Where(p => p.EnrollmentId == enrollmentId)
      .FirstOrDefaultAsync();

    if (pathwayTypeEnrollments == null) return new NotFoundObjectResult("Did not find any enrollments");

    return new OkObjectResult(pathwayTypeEnrollments);
  }

  [Function("CreatePathwayEnrollment")]
  public async Task<IActionResult> CreatePathwayEnrollment([HttpTrigger(AuthorizationLevel.Function, "post", Route = "participants/pathwayEnrollment")]
    HttpRequestData req)
  {
    _logger.LogInformation("C# HTTP trigger function CreatePathwayEnrollment processed a request.");

    CreateParticipantEnrollmentDto participantEnrollmentDto = null;

    try
    {
      JsonSerializerOptions options = new()
      {
        ReferenceHandler = ReferenceHandler.Preserve
      };

      participantEnrollmentDto = JsonSerializer.Deserialize<CreateParticipantEnrollmentDto>(req.Body.ToString(), options);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unable to deserialize event data to CreateParticipantEnrollmentDto object.");
    }

      // var response = await httpClient.PostAsJsonAsync($"/api/participants", participantEnrollmentDto);
      // response.EnsureSuccessStatusCode();

    var pathwayTypeEnrollment = new PathwayTypeEnrollment(){
      EnrollmentId = Guid.NewGuid(),
      ScreeningName = "",
      PathwayName = participantEnrollmentDto?.PathwayName ?? "",
      Status = ""
    };

    var result = await _dbContext.PathwayTypeEnrollments.AddAsync(pathwayTypeEnrollment);

    return new CreatedResult($"pathwaytypeenrollments/", pathwayTypeEnrollment);
  }
}
