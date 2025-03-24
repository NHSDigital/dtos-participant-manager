using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.Sensitive;
using Serilog.Formatting.Compact;

namespace ParticipantManager.Shared.Extensions;

public static class HostBuilderExtensions
{
  public static IHostBuilder ConfigureSerilogLogging(this IHostBuilder hostBuilder, string appInsightsConnectionString)
  {
    return hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
    {
      loggerConfiguration
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .Enrich.WithCorrelationIdHeader("X-Correlation-ID")
        .Destructure.With(new NhsNumberHashingPolicy()) // Apply NHS number hashing by default
        .Enrich.WithSensitiveDataMasking(options =>
        {
          options.MaskingOperators
            .Clear(); // Clearing default masking operators to prevent GUIDs being masked unintentionally
          options.MaskingOperators.Add(new NhsNumberRegexMaskOperator());
          options.MaskingOperators.Add(new EmailAddressMaskingOperator());
        })
        .WriteTo.Console(new RenderedCompactJsonFormatter())
        .WriteTo.ApplicationInsights(appInsightsConnectionString, TelemetryConverter.Traces);
    });
  }
}
