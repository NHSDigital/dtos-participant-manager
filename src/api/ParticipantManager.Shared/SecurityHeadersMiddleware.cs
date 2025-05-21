using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace ParticipantManager.Shared
{
    public class SecurityHeadersMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            await next(context);

            var httpResponseData = context.GetHttpResponseData();

            if (httpResponseData != null)
            {
                httpResponseData.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self'; object-src 'none'; frame-ancestors 'none'");
                httpResponseData.Headers.Add("X-Frame-Options", "DENY");
                httpResponseData.Headers.Add("X-Content-Type-Options", "nosniff");
                httpResponseData.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
                httpResponseData.Headers.Add("X-XSS-Protection", "1; mode=block");
                httpResponseData.Headers.Add("Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=()");
                httpResponseData.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            }
        }
    }
}
