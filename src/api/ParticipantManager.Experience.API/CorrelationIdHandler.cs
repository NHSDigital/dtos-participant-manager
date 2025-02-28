namespace ParticipantManager.Experience.API;

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

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

    if (context != null && context.Items.TryGetValue("x-correlation-id", out var correlationId) && correlationId is string correlationValue)
    {
      if (!request.Headers.Contains("x-correlation-id"))
      {
        request.Headers.Add("x-correlation-id", correlationValue);
      }
    }

    return await base.SendAsync(request, cancellationToken);
  }
}
