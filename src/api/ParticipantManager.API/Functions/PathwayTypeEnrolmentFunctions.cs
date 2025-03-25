using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParticipantManager.API.Data;
using ParticipantManager.API.Models;

namespace ParticipantManager.API.Functions;

public class PathwayTypeEnrolmentFunctions(
    ILogger<PathwayTypeEnrolmentFunctions> logger,
    ParticipantManagerDbContext dbContext,
    JsonSerializerOptions jsonSerializerOptions)
{
    [Function("GetPathwayTypeEnrolmentsByParticipantId")]
    public async Task<IActionResult> GetPathwayTypeEnrolmentsByParticipantId(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "pathwaytypeenrolments")]
        HttpRequest req)
    {
        logger.LogInformation($"{nameof(GetPathwayTypeEnrolmentsByParticipantId)} processed a request.");

        var participantId = req.Query["participantId"].ToString();

        if (string.IsNullOrEmpty(participantId)) return new BadRequestObjectResult("Missing ParticipantId");

        var pathwayTypeEnrolments = await dbContext.PathwayTypeEnrolments
            .Where(p => p.ParticipantId == Guid.Parse(participantId))
            .Select(p => new PathwayTypeEnrolment
            {
                EnrolmentId = p.EnrolmentId,
                EnrolmentDate = p.EnrolmentDate,
                PathwayTypeName = p.PathwayTypeName,
                ScreeningName = p.ScreeningName,
                PathwayTypeId = p.PathwayTypeId,
                Status = p.Status,
                LapsedDate = p.LapsedDate,
                NextActionDate = p.NextActionDate,
                Episodes = p.Episodes,
                ParticipantId = p.ParticipantId,
                Participant = new Participant
                {
                    NhsNumber = p.Participant.NhsNumber, ParticipantId = p.Participant.ParticipantId,
                    Name = p.Participant.Name
                }
            })
            .ToListAsync();

        return new OkObjectResult(pathwayTypeEnrolments);
    }

    [Function("GetPathwayTypeEnrolmentById")]
    public async Task<IActionResult> GetPathwayTypeEnrolmentById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get",
            Route = "participants/{participantId:guid}/pathwaytypeenrolments/{enrolmentId:guid}")]
        HttpRequestData req, Guid participantId, Guid enrolmentId)
    {
        logger.LogInformation($"{nameof(GetPathwayTypeEnrolmentById)} processed a request.");

        var pathwayTypeEnrolments = await dbContext.PathwayTypeEnrolments
            .Where(p => p.EnrolmentId == enrolmentId && p.ParticipantId == participantId)
            .Select(p => new PathwayTypeEnrolment
            {
                EnrolmentId = p.EnrolmentId,
                EnrolmentDate = p.EnrolmentDate,
                PathwayTypeName = p.PathwayTypeName,
                ScreeningName = p.ScreeningName,
                PathwayTypeId = p.PathwayTypeId,
                Status = p.Status,
                LapsedDate = p.LapsedDate,
                NextActionDate = p.NextActionDate,
                Episodes = p.Episodes,
                ParticipantId = p.ParticipantId,
                Participant = new Participant
                {
                    NhsNumber = p.Participant.NhsNumber, ParticipantId = p.Participant.ParticipantId,
                    Name = p.Participant.Name
                }
            }).FirstOrDefaultAsync();

        if (pathwayTypeEnrolments == null) return new NotFoundObjectResult("Did not find any enrolments");

        return new OkObjectResult(pathwayTypeEnrolments);
    }

    [Function("CreatePathwayTypeEnrolment")]
    public async Task<IActionResult> CreatePathwayTypeEnrolment(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "pathwaytypeenrolment")]
        HttpRequestData req)
    {
        logger.LogInformation($"{nameof(CreatePathwayTypeEnrolment)} processed a request.");

        PathwayTypeEnrolment? pathwayTypeEnrolment;
        var validationResults = new List<ValidationResult>();

        try
        {
            pathwayTypeEnrolment =
                await JsonSerializer.DeserializeAsync<PathwayTypeEnrolment>(req.Body, jsonSerializerOptions);

            if (pathwayTypeEnrolment == null)
            {
                logger.LogError("Invalid pathwayTypeEnrolment JSON provided. Deserialized to null.");
                return new BadRequestObjectResult("Invalid pathwayTypeEnrolment JSON provided. Deserialized to null.");
            }

            var context = new ValidationContext(pathwayTypeEnrolment, null, null);
            if (!Validator.TryValidateObject(pathwayTypeEnrolment, context, validationResults, true))
            {
                var errorMessages = string.Join("; ", validationResults.Select(vr => vr.ErrorMessage));
                throw new ValidationException($"Failed to validate PathwayTypeEnrolment: {errorMessages}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to deserialise/validate PathwayTypeEnrolment object");
            return new BadRequestObjectResult(validationResults);
        }

        try
        {
            await dbContext.PathwayTypeEnrolments.AddAsync(pathwayTypeEnrolment);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Successfully created PathwayTypeEnrolment for Participant",
                pathwayTypeEnrolment.ParticipantId);
            return new CreatedResult($"pathwaytypeenrolments/{pathwayTypeEnrolment.EnrolmentId}", pathwayTypeEnrolment);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save PathwayTypeEnrolment to the database");
            return new StatusCodeResult(500);
        }
    }
}
