using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ParticipantManager.API.Data;
using ParticipantManager.Shared.Extensions;

var appInsightsConnectionString =
  Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING") ?? string.Empty;
var databaseConnectionString = Environment.GetEnvironmentVariable("ParticipantManagerDatabaseConnectionString");


var host = new HostBuilder()
  .ConfigureFunctionsWebApplication()
  .ConfigureServices((context, services) =>
  {
    services.AddOpenTelemetry()
      .ConfigureResource(builder => builder
        .AddService("ParticipantManager.API"))
      .WithTracing(builder => builder
        .AddSource(nameof(ParticipantManager.API))
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddAzureMonitorTraceExporter(options => { options.ConnectionString = appInsightsConnectionString; }))
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
        throw new InvalidOperationException("The connection string has not been initialized.");

      options.UseSqlServer(databaseConnectionString);
    });
    services.AddHttpContextAccessor();
  })
  .ConfigureSerilogLogging(appInsightsConnectionString)
  .ConfigureLogging(logging =>
  {
    logging.AddOpenTelemetry(options =>
    {
      options.AddAzureMonitorLogExporter(options => { options.ConnectionString = appInsightsConnectionString; });
    });
  })
  .Build();

await host.RunAsync();
