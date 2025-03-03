using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ParticipantManager.EventHandler.Functions;

public class CreateEnrollmentHandler
{
  private readonly ILogger<CreateEnrollmentHandler> _logger;

  public CreateEnrollmentHandler(ILogger<CreateEnrollmentHandler> logger)
  {
    _logger = logger;
  }

  [Function("CreateEnrollmentHandler")]
  public void Run([EventGridTrigger] EventGridEvent eventGridEvent)
  {

    _logger.LogInformation("Event type: {type}, Event subject: {subject}", eventGridEvent.GetType(), eventGridEvent.Subject);
  }
}
