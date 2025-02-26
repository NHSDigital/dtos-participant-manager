using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Enrichers.Sensitive;
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using ParticipantManager.Shared;
using Microsoft.EntityFrameworkCore;
using ParticipantManager.API.Data;

string appInsightsConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING") ?? string.Empty;
string databaseConnectionString = Environment.GetEnvironmentVariable("ParticipantManagerDatabaseConnectionString");



var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
      services.AddOpenTelemetry()
      .ConfigureResource(builder => builder
      .AddService(serviceName: "ParticipantManager.API"))
      .WithTracing(builder => builder
      .AddSource(nameof(ParticipantManager.API))
      .AddHttpClientInstrumentation()
      .AddAspNetCoreInstrumentation()
      .AddAzureMonitorTraceExporter(options =>
      {
        options.ConnectionString = appInsightsConnectionString;
      }))
      .WithMetrics(builder => builder
      .AddMeter(nameof(ParticipantManager.API))
      .AddHttpClientInstrumentation()
      .AddAspNetCoreInstrumentation()
      .AddAzureMonitorMetricExporter(options =>
      {
        options.ConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
      }));

      services.AddDbContext<ParticipantManagerDbContext>(options =>
          {
            if (string.IsNullOrEmpty(databaseConnectionString))
            {
              throw new InvalidOperationException("The connection string has not been initialized.");
            }

            options.UseSqlServer(databaseConnectionString);
          });
    })
    .UseSerilog((context, services, loggerConfiguration) =>
    {
      loggerConfiguration
          .MinimumLevel.Information()
          .Enrich.FromLogContext()
          .Destructure.With(new NhsNumberHashingPolicy()) // Apply NHS number hashing by default
          .Enrich.WithSensitiveDataMasking(options =>
          {
            options.MaskingOperators.Clear(); // Clearing default masking operators to prevent GUIDs being masked unintentionally
            options.MaskingOperators.Add(new NhsNumberRegexMaskOperator());
            options.MaskingOperators.Add(new EmailAddressMaskingOperator());
          })
          .WriteTo.Console(new Serilog.Formatting.Compact.RenderedCompactJsonFormatter())
          .WriteTo.ApplicationInsights(appInsightsConnectionString, TelemetryConverter.Traces);
    })
    .ConfigureLogging(logging =>
    {
      logging.AddOpenTelemetry(options =>
      {
        options.AddAzureMonitorLogExporter(options =>
          {
            options.ConnectionString = appInsightsConnectionString;
          });
      });
    })
    .Build();

await host.RunAsync();
