using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using System.Net.Http.Headers;

public class CorrelationIdMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpRequest = await context.GetHttpRequestDataAsync();
        if (httpRequest != null)
        {
            // Extract correlation ID or generate one
            string correlationId = httpRequest.Headers.TryGetValues("x-correlation-id", out var values)
                ? values.FirstOrDefault()
                : Guid.NewGuid().ToString();

            // Store in FunctionContext Items for later use
            context.Items["CorrelationId"] = correlationId;
        }

        await next(context);
    }
}
