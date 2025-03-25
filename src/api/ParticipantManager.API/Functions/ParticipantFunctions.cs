using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParticipantManager.API.Data;
using ParticipantManager.API.Models;

namespace ParticipantManager.API.Functions;

public class ParticipantFunctions
{
    private readonly ParticipantManagerDbContext _dbContext;
    private readonly ILogger<ParticipantFunctions> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public ParticipantFunctions(ILogger<ParticipantFunctions> logger, ParticipantManagerDbContext dbContext,
        JsonSerializerOptions jsonSerializerOptions)
    {
        _logger = logger;
        _dbContext = dbContext;
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    [Function("CreateParticipant")]
    public async Task<IActionResult> CreateParticipant(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "participants")]
        HttpRequestData req)
    {
        _logger.LogInformation("{CreateParticipant} function processed a request.", nameof(CreateParticipant));

        try
        {
            // Deserialize the JSON request body into the Participant model
            var participant = await JsonSerializer.DeserializeAsync<Participant>(req.Body, _jsonSerializerOptions);
            if (participant == null)
            {
                _logger.LogError("Invalid participant JSON provided. Deserialized to null.");
                return new BadRequestObjectResult("Invalid participant JSON provided. Deserialized to null.");
            }

            // Validate Data Annotations
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(participant, null, null);

            if (!Validator.TryValidateObject(participant, context, validationResults, true))
            {
                _logger.LogError("Validation failed for Participant creation.");
                return new BadRequestObjectResult(validationResults);
            }

            // Check if a Participant with the same NHS Number already exists
            var existingParticipant = await _dbContext.Participants
                .FirstOrDefaultAsync(p => p.NhsNumber == participant.NhsNumber);

            if (existingParticipant != null)
            {
                _logger.LogError("Attempted to create a duplicate Participant with NHS Number: {@NhsNumber}",
                    new { NhsNumber = participant.NhsNumber });
                return new ConflictObjectResult(new { message = "A Participant with this NHS Number already exists." });
            }

            _dbContext.Participants.Add(participant);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Successfully Created Participant, Participant: {@Participant}",
                new { NhsNumber = participant.NhsNumber, participant.ParticipantId });
            return new CreatedResult($"/participants/{participant.ParticipantId}", participant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Participant} function processed a request and an exception was thrown.",
                nameof(CreateParticipant));
            return new BadRequestObjectResult(new { message = "An error occurred while processing the request." });
        }
    }


    [Function("GetParticipantById")]
    public async Task<IActionResult> GetParticipantById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "participants/{participantId:guid}")]
        HttpRequestData req, Guid participantId)
    {
        _logger.LogInformation("Fetching Participant with ID: {ParticipantId}", participantId);

        var participant = await _dbContext.Participants.FindAsync(participantId);
        if (participant == null)
        {
            _logger.LogError("Participant not found: {ParticipantId}", participantId);
            return new NotFoundObjectResult($"Participant with ID {participantId} not found.");
        }

        return new OkObjectResult(participant);
    }


    [Function("GetParticipantByNhsNumber")]
    public async Task<IActionResult> GetParticipantByNhsNumber(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "participants")]
        HttpRequestData req)
    {
        _logger.LogInformation("Processing Participant search request.");

        // Extract query parameters
        var queryParams = HttpUtility.ParseQueryString(req.Url.Query);
        var nhsNumber = queryParams["nhsNumber"];

        if (string.IsNullOrEmpty(nhsNumber))
        {
            _logger.LogError("NHS Number not provided.");
            return new BadRequestObjectResult("Please provide an NHS Number.");
        }

        var participant = await _dbContext.Participants
            .FirstOrDefaultAsync(p => p.NhsNumber == nhsNumber);

        if (participant == null)
        {
            _logger.LogError("Participant with NHS Number {@NhsNumber} not found.", new { NhsNumber = nhsNumber });
            return new NotFoundObjectResult($"No Participant found with NHS Number {nhsNumber}.");
        }

        return new OkObjectResult(participant);
    }
}
