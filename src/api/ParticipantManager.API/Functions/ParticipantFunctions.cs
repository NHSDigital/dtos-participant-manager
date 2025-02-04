using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using ParticipantManager.API.Data;
using ParticipantManager.API.Models;

namespace ParticipantManager.API.Functions;

public class ParticipantFunctions
{
  private readonly ILogger<ParticipantFunctions> _logger;
  private readonly ParticipantManagerDbContext _dbContext;

  public ParticipantFunctions(ILogger<ParticipantFunctions> logger, ParticipantManagerDbContext dbContext)
  {
    _logger = logger;
    _dbContext = dbContext;
  }

  [Function("CreateParticipant")]
  public async Task<IActionResult> CreateParticipant(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "participants")] HttpRequestData req)
  {
    _logger.LogInformation("C# HTTP trigger function processed a request.");

    try
    {
      // Deserialize the JSON request body into the Participant model
      var participant = await JsonSerializer.DeserializeAsync<Participant>(req.Body,
        new JsonSerializerOptions
        {
          PropertyNameCaseInsensitive = true
        });
      // Validate Data Annotations
      var validationResults = new List<ValidationResult>();
      var context = new ValidationContext(participant, serviceProvider: null, items: null);

      if (!Validator.TryValidateObject(participant, context, validationResults, true))
      {
        _logger.LogWarning("Validation failed for participant creation.");
        return new BadRequestObjectResult(validationResults);
      }
      // Check if a participant with the same NHS Number already exists
      var existingParticipant = await _dbContext.Participants
        .FirstOrDefaultAsync(p => p.NHSNumber == participant.NHSNumber);

      if (existingParticipant != null)
      {
        _logger.LogWarning("Attempted to create a duplicate participant with NHS Number: {NHSNumber}", participant.NHSNumber);
        return new ConflictObjectResult(new { message = "A participant with this NHS Number already exists." });
      }

      _dbContext.Participants.Add(participant);
      await _dbContext.SaveChangesAsync();

      return new CreatedResult($"/participants/{participant.ParticipantId}", participant);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "C# HTTP trigger function processed a request and an exception was thrown.");
      return new BadRequestObjectResult(new { message = "An error occurred while processing the request." });
    }
  }


  [Function("GetParticipantById")]
  public async Task<IActionResult> GetParticipantById(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "participants/{participantId:guid}")]
    HttpRequestData req, Guid participantId)
  {
    _logger.LogInformation("Fetching participant with ID: {ParticipantId}", participantId);

    var participant = await _dbContext.Participants.FindAsync(participantId);
    if (participant == null)
    {
      _logger.LogWarning("Participant not found: {ParticipantId}", participantId);
      return new NotFoundObjectResult($"Participant with ID {participantId} not found.");
    }
    return new OkObjectResult(participant);
  }


  [Function("GetParticipantByNhsNumber")]
  public async Task<IActionResult> GetParticipantByNhsNumber(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "participants")]
    HttpRequestData req)
  {
    _logger.LogInformation("Processing participant search request.");

    // Extract query parameters
    var queryParams = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
    string? nhsNumber = queryParams["nhsNumber"];

    if (string.IsNullOrEmpty(nhsNumber))
    {
      _logger.LogWarning("NHS Number not provided.");
      return new BadRequestObjectResult("Please provide an NHS Number.");
    }

    var participant = await _dbContext.Participants
      .FirstOrDefaultAsync(p => p.NHSNumber == nhsNumber);

    if (participant == null)
    {
      _logger.LogWarning("Participant with NHS Number {NhsNumber} not found.", nhsNumber);
      return new NotFoundObjectResult($"No participant found with NHS Number {nhsNumber}.");
    }

    return new OkObjectResult(participant);
  }



}


