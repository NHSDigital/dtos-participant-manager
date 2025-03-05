using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace ParticipantManager.Shared;

public class CorrelationIdMiddleware : IFunctionsWorkerMiddleware
{
  private const string CorrelationIdHeader = "X-Correlation-ID";
  private readonly FunctionContextAccessor _functionContextAccessor;

  public CorrelationIdMiddleware(FunctionContextAccessor contextAccessor)
  {
    _functionContextAccessor = contextAccessor;
  }

  public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
  {
    var logger = context.GetLogger<CorrelationIdMiddleware>();
    _functionContextAccessor.FunctionContext = context;
    if (context.BindingContext.BindingData.TryGetValue("Headers", out var headersObj))
    {
      var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersObj.ToString());
      if (headers.TryGetValue(CorrelationIdHeader, out var correlationId) && !string.IsNullOrEmpty(correlationId))
      {
        logger.LogInformation("Using existing correlation ID: {CorrelationId}", correlationId);
        context.Items[CorrelationIdHeader] = correlationId;
      }
      else
      {
        correlationId = Guid.NewGuid().ToString();
        logger.LogInformation("Generated new correlation ID: {CorrelationId}", correlationId);
        context.Items[CorrelationIdHeader] = correlationId;
      }
    }
    else
    {
      var correlationId = Guid.NewGuid().ToString();
      logger.LogInformation("Generated correlation ID as headers were missing: {CorrelationId}", correlationId);
      context.Items[CorrelationIdHeader] = correlationId;
    }

    await next(context);
  }
}
