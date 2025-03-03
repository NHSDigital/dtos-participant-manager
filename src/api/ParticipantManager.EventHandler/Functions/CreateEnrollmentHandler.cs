using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ParticipantManager.Shared.Client;
using ParticipantManager.Shared.DTOs;

namespace ParticipantManager.EventHandler.Functions;

public class CreateEnrollmentHandler
{
  private readonly ILogger<CreateEnrollmentHandler> _logger;
  private readonly ICrudApiClient _crudApiClient;

  public CreateEnrollmentHandler(ILogger<CreateEnrollmentHandler> logger, ICrudApiClient crudApiClient)
  {
    _logger = logger;
    _crudApiClient = crudApiClient;
  }

  [Function("CreateEnrollmentHandler")]
  public void Run([EventGridTrigger] EventGridEvent eventGridEvent)
  {
    _logger.LogInformation("Event type: {type}, Event subject: {subject}", eventGridEvent.GetType(),
      eventGridEvent.Subject);

    string serializedEvent = JsonSerializer.Serialize(eventGridEvent);
    _logger.LogInformation(serializedEvent);

    CreateParticipantDto participantDto;

    try
    {
      JsonSerializerOptions options = new()
      {
        ReferenceHandler = ReferenceHandler.Preserve
      };

      participantDto = JsonSerializer.Deserialize<CreateParticipantDto>(eventGridEvent.Data.ToString(), options);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unable to deserialize event data to Episode object.");
      return;
    }

    _crudApiClient.CreateEnrollmentAsync(participantDto);
  }
}
