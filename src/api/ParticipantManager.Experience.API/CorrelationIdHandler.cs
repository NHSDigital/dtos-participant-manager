using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using AzureFunctions.Extensions.Middleware.Abstractions;

public class CorrelationIdHandler : DelegatingHandler
{
    private readonly FunctionContextAccessor _httpContextAccessor;

    public CorrelationIdHandler(FunctionContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Check if the correlation ID is present in the incoming request
        if (_httpContextAccessor.FunctionContext.Items.ContainsKey("X-Correlation-ID"))
        {
            // Add the correlation ID to the outgoing request
            string correlationId = _httpContextAccessor.FunctionContext.Items["X-Correlation-ID"].ToString();
            request.Headers.Add("X-Correlation-ID", correlationId);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
