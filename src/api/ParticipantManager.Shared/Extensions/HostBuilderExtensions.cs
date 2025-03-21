using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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

  public static IHostBuilder ConfigureOpenTelemetry(this IHostBuilder hostBuilder, string sourceName, string appInsightsConnectionString)
  {
    return hostBuilder.ConfigureServices((context, services) =>
    {
      services.AddOpenTelemetry()
        .ConfigureResource(builder => builder
          .AddService("ParticipantManager.API"))
        .WithTracing(builder => builder
          .AddSource(sourceName)
          .AddHttpClientInstrumentation()
          .AddAspNetCoreInstrumentation()
          .AddAzureMonitorTraceExporter(options => { options.ConnectionString = appInsightsConnectionString; }))
        .WithMetrics(builder => builder
          .AddMeter(sourceName)
          .AddHttpClientInstrumentation()
          .AddAspNetCoreInstrumentation()
          .AddAzureMonitorMetricExporter(options =>
          {
            options.ConnectionString = appInsightsConnectionString;
          }));
    });
  }
}
