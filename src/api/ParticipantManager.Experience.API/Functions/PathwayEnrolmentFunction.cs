using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ParticipantManager.Experience.API.Services;
using ParticipantManager.Shared.Client;

namespace ParticipantManager.Experience.API.Functions;

public class PathwayEnrolmentFunction(
    ILogger<PathwayEnrolmentFunction> logger,
    ICrudApiClient crudApiClient,
    ITokenService tokenService,
    IFeatureFlagClient featureFlagClient)
{
    [Function("GetPathwayEnrolmentById")]
    public async Task<IActionResult> GetPathwayEnrolmentById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get",
            Route = "participants/{participantId}/pathwayenrolments/{enrolmentid}")]
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

            var pathwayEnrolment = await crudApiClient.GetPathwayEnrolmentByIdAsync(participantId, enrolmentId);
            if (pathwayEnrolment == null)
            {
                logger.LogError("Failed to find pathway enrolment for Request {@Request}",
                    new { NhsNumber = nhsNumber, EnrolmentId = enrolmentId });
                return new NotFoundResult();
            }

            if (pathwayEnrolment.Participant.NhsNumber != nhsNumber)
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

            logger.LogInformation("Found pathway enrolment for Request {@Request}",
                new { NhsNumber = nhsNumber, EnrolmentId = enrolmentId });
            return new OkObjectResult(pathwayEnrolment);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Invalid: Bad Request");
            return new BadRequestObjectResult(ex.Message);
        }
    }
}
