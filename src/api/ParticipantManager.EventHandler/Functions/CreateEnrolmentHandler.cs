using System.Net;
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
  private readonly EventGridPublisherClient _eventGridPublisherClient;

  public CreateEnrolmentHandler(ILogger<CreateEnrolmentHandler> logger, ICrudApiClient crudApiClient, EventGridPublisherClient eventGridPublisherClient)
  {
    _logger = logger;
    _crudApiClient = crudApiClient;
    _eventGridPublisherClient = eventGridPublisherClient;
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

    var pathwayEnrolmentDto = new CreatePathwayTypeEnrolmentDto
    {
      ParticipantId = participantId.Value,
      PathwayTypeId = pathwayParticipantDto.PathwayTypeId,
      PathwayTypeName = pathwayParticipantDto.PathwayTypeName,
      ScreeningName = pathwayParticipantDto.ScreeningName
    };

    var succeeded = await _crudApiClient.CreatePathwayTypeEnrolmentAsync(pathwayEnrolmentDto);

    if (!succeeded)
    {
      _logger.LogError("Failed to create PathwayTypeEnrollment for Participant: {Participant}, PathwayTypeName: {PathwayTypeName}",
      pathwayEnrolmentDto.ParticipantId, pathwayEnrolmentDto.PathwayTypeName);
      return;
    }

    // Send Event
    var eventToSend = new EventGridEvent(
      subject: "PathwayTypeEnrolment",
      eventType: "Created",
      dataVersion: "1.0",
      data: "Test"
    );

    var result = await _eventGridPublisherClient.SendEventAsync(eventToSend);

    if (result.Status != (int)HttpStatusCode.OK)
    {
      _logger.LogError("Send event has failed, Event details: {SendEvent}", eventToSend);
    }
  }
}
