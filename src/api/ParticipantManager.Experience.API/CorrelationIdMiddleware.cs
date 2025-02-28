using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace ParticipantManager.Experience.API;

public class CorrelationIdMiddleware : IFunctionsWorkerMiddleware
{
  private readonly FunctionContextAccessor _functionContextAccessor;
  private const string CorrelationIdHeader = "x-correlation-id";

  public CorrelationIdMiddleware(FunctionContextAccessor contextAccessor)
  {
    _functionContextAccessor = contextAccessor;
  }



  public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
  {
    var logger = context.GetLogger<CorrelationIdMiddleware>();
    _functionContextAccessor.FunctionContext = context;
    // Extract headers from the BindingContext
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
        // Generate a new correlation ID if missing
        correlationId = Guid.NewGuid().ToString();
        logger.LogInformation("Generated new correlation ID: {CorrelationId}", correlationId);
        context.Items[CorrelationIdHeader] = correlationId;
      }
    }
    else
    {
      // Generate a correlation ID if headers are not present
      string correlationId = Guid.NewGuid().ToString();
      logger.LogInformation("Generated correlation ID as headers were missing: {CorrelationId}", correlationId);
      context.Items[CorrelationIdHeader] = correlationId;
    }

    // Continue function execution
    await next(context);
  }
}
