namespace ParticipantManager.Experience.API;

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class CorrelationIdHandler : DelegatingHandler
{
  private readonly FunctionContextAccessor _functionContextAccessor;

  public CorrelationIdHandler(FunctionContextAccessor functionContextAccessor)
  {
    _functionContextAccessor = functionContextAccessor;
  }

  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
  {
    var context = _functionContextAccessor.FunctionContext;

    if (context != null && context.Items.TryGetValue("X-Correlation-ID", out var correlationId) && correlationId is string correlationValue)
    {
      if (!request.Headers.Contains("X-Correlation-ID"))
      {
        request.Headers.Add("X-Correlation-ID", correlationValue);
      }
    }

    return await base.SendAsync(request, cancellationToken);
  }
}
