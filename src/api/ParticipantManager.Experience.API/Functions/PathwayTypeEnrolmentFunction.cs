using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ParticipantManager.Experience.API.Services;
using ParticipantManager.Shared.Client;

namespace ParticipantManager.Experience.API.Functions;

public class PathwayTypeEnrolmentFunction(
    ILogger<PathwayTypeEnrolmentFunction> logger,
    ICrudApiClient crudApiClient,
    ITokenService tokenService,
    IFeatureFlagClient featureFlagClient)
{
    [Function("GetPathwayTypeEnrolmentById")]
    public async Task<IActionResult> GetPathwayTypeEnrolmentById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get",
            Route = "participants/{participantId}/pathwaytypeenrolments/{enrolmentid}")]
        HttpRequestData req, Guid participantId, Guid enrolmentId)
    {
        try
        {
            var result = await tokenService.ValidateToken(req);

            if (result.Status != AccessTokenStatus.Valid)
            {
                logger.LogError("Invalid access token");
                return new UnauthorizedResult();
            }

            logger.LogInformation("Access token is valid, looking for NHS Number");

            var nhsNumber = result.Principal?.Claims.FirstOrDefault(c => c.Type == "nhs_number")?.Value;
            if (string.IsNullOrEmpty(nhsNumber))
            {
                logger.LogError("Access token doesn't contain NHS number");
                return new UnauthorizedResult();
            }

            var pathwayTypeEnrolment = await crudApiClient.GetPathwayTypeEnrolmentByIdAsync(participantId, enrolmentId);
            if (pathwayTypeEnrolment == null)
            {
                logger.LogError("Failed to find pathway type enrolment for Request {@Request}",
                    new { NhsNumber = nhsNumber, EnrolmentId = enrolmentId });
                return new NotFoundResult();
            }

            if (pathwayTypeEnrolment.Participant.NhsNumber != nhsNumber)
            {
                logger.LogError("Logged in user does not have access to this record: {@ParticipantId}",
                    new { ParticipantId = participantId });
                return new ForbidResult();
            }

            var enabled = await featureFlagClient.IsFeatureEnabledForParticipant("mays_mvp", participantId);

            if (!enabled)
            {
                return new ForbidResult();
            }

            logger.LogInformation("Found pathway type enrolment for Request {@Request}",
                new { NhsNumber = nhsNumber, EnrolmentId = enrolmentId });
            return new OkObjectResult(pathwayTypeEnrolment);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Invalid: Bad Request");
            return new BadRequestObjectResult(ex.Message);
        }
    }
}
