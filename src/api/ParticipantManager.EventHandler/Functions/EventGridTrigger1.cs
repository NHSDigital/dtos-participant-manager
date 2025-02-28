using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ParticipantManager.EventHandler.Functions;

public class EventGridTrigger1
{
  private readonly ILogger<EventGridTrigger1> _logger;

  public EventGridTrigger1(ILogger<EventGridTrigger1> logger)
  {
    _logger = logger;
  }

  [Function("MockProductHandler")]
  public void Run([EventGridTrigger] EventGridEvent eventGridEvent)
  {
    _logger.LogInformation("Event type: {type}, Event subject: {subject}", eventGridEvent.GetType(), eventGridEvent.Subject);
  }
}
