using System.Text.Json;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ParticipantManager.Shared.Client;
using ParticipantManager.Shared.DTOs;

namespace ParticipantManager.EventHandler.Functions;

public class CreateEnrolmentHandler
{
  private readonly ILogger<CreateEnrolmentHandler> _logger;
  private readonly ICrudApiClient _crudApiClient;

  public CreateEnrolmentHandler(ILogger<CreateEnrolmentHandler> logger, ICrudApiClient crudApiClient)
  {
    _logger = logger;
    _crudApiClient = crudApiClient;
  }

  [Function("CreateEnrolmentHandler")]
  public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
  {
    _logger.LogInformation("Event type: {Type}, Event subject: {Subject}", eventGridEvent.GetType(),
      eventGridEvent.Subject);

    CreatePathwayParticipantDto? pathwayParticipantDto;

    try
    {
      pathwayParticipantDto = JsonSerializer.Deserialize<CreatePathwayParticipantDto>(eventGridEvent.Data.ToString());
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unable to deserialize event data to CreateParticipantEnrolmentDto.");
      return;
    }

    if (pathwayParticipantDto == null)
    {
      return;
    }

    var participantDto = await _crudApiClient.GetParticipantByNhsNumberAsync(pathwayParticipantDto.NHSNumber);

    var participantId = participantDto == null
    ? await _crudApiClient.CreateParticipantAsync(pathwayParticipantDto)
    : participantDto.ParticipantId;

    var pathwayEnrolmentDto = new CreatePathwayEnrolmentDto
    {
      ParticipantId = participantId.Value,
      PathwayTypeId = pathwayParticipantDto.PathwayTypeId,
      PathwayTypeName = pathwayParticipantDto.PathwayTypeName
    };

    await _crudApiClient.CreatePathwayEnrolmentAsync(pathwayEnrolmentDto);
  }
}
