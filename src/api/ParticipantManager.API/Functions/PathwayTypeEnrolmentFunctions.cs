using System.Text.Json;
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

  [Function("GetPathwayTypeEnrolmentsByNhsNumber")]
  public async Task<IActionResult> GetPathwayTypeEnrolmentsByNhsNumber(
    [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "pathwaytypeenrolments")]
    HttpRequest req)
  {
    _logger.LogInformation($"{nameof(GetPathwayTypeEnrolmentsByNhsNumber)} processed a request.");

    var nhsNumber = req.Query["nhsnumber"].ToString();

    if (string.IsNullOrEmpty(nhsNumber)) return new BadRequestObjectResult("Missing NHS Number");

    var pathwayTypeEnrolments = await _dbContext.PathwayTypeEnrolments
      .Where(p => p.Participant.NHSNumber == nhsNumber)
      .ToListAsync();

    if (pathwayTypeEnrolments == null) return new NotFoundObjectResult("Did not find any enrolments");

    return new OkObjectResult(pathwayTypeEnrolments);
  }

  [Function("GetPathwayTypeEnrolmentById")]
  public async Task<IActionResult> GetPathwayTypeEnrolmentById(
    [HttpTrigger(AuthorizationLevel.Function, "get",
      Route = "pathwaytypeenrolments/{enrolmentId:guid}")]
    HttpRequestData req, Guid enrolmentId)
  {
    _logger.LogInformation($"{nameof(GetPathwayTypeEnrolmentById)} processed a request.");

    var pathwayTypeEnrolments = await _dbContext.PathwayTypeEnrolments
      .Where(p => p.EnrolmentId == enrolmentId)
      .FirstOrDefaultAsync();

    if (pathwayTypeEnrolments == null) return new NotFoundObjectResult("Did not find any enrolments");

    return new OkObjectResult(pathwayTypeEnrolments);
  }

  [Function("CreatePathwayTypeEnrolment")]
  public async Task<IActionResult> CreatePathwayTypeEnrolment([HttpTrigger(AuthorizationLevel.Function, "post", Route = "pathwaytypeenrolment")]
    HttpRequestData req)
  {
    _logger.LogInformation($"{nameof(CreatePathwayTypeEnrolment)} processed a request.");

    CreatePathwayTypeEnrolmentDto participantEnrolmentDto = new CreatePathwayTypeEnrolmentDto();

    try
    {
      participantEnrolmentDto = await JsonSerializer.DeserializeAsync<CreatePathwayTypeEnrolmentDto>(req.Body,
        new JsonSerializerOptions
        {
          PropertyNameCaseInsensitive = true
        });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unable to deserialize event data to CreateParticipantEnrolmentDto object.");
    }

    var pathwayTypeEnrolment = new PathwayTypeEnrolment()
    {
      EnrolmentId = Guid.NewGuid(),
      EnrolmentDate = DateTime.UtcNow,
      ParticipantId = participantEnrolmentDto.ParticipantId,
      PathwayTypeId = participantEnrolmentDto.PathwayTypeId,
      ScreeningName = participantEnrolmentDto.ScreeningName,
      PathwayTypeName = participantEnrolmentDto?.PathwayTypeName ?? "",
      Status = ""
    };

    await _dbContext.PathwayTypeEnrolments.AddAsync(pathwayTypeEnrolment);
    await _dbContext.SaveChangesAsync();

    return new CreatedResult($"pathwaytypeenrolments/{pathwayTypeEnrolment.EnrolmentId}", pathwayTypeEnrolment);
  }
}
