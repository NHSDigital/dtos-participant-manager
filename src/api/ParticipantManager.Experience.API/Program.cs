using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParticipantManager.Experience.API.Client;
using Microsoft.Extensions.Logging;
using ParticipantManager.Experience.API.Services;
using Serilog;
using Serilog.Enrichers.Sensitive;
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using ParticipantManager.Shared;

string appInsightsConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING") ?? string.Empty;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
      services.AddOpenTelemetry()
      .ConfigureResource(builder => builder
      .AddService(serviceName: "ParticipantManager.Experience.API"))
      .WithTracing(builder => builder
      .AddSource(nameof(ParticipantManager.Experience.API))
      .AddHttpClientInstrumentation()
      .AddAspNetCoreInstrumentation()
      .AddAzureMonitorTraceExporter(options =>
      {
        options.ConnectionString = appInsightsConnectionString;
      }))
          .WithMetrics(builder => builder
              .AddMeter(nameof(ParticipantManager.Experience.API))
              .AddHttpClientInstrumentation()
              .AddAspNetCoreInstrumentation()
              .AddAzureMonitorMetricExporter(options =>
              {
                options.ConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
              }));

      services.AddHttpClient<ICrudApiClient, CrudApiClient>((sp, client) =>
      {
        client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("CRUD_API_URL") ?? string.Empty);
      });

      services.AddSingleton<IJwksProvider>(provider =>
      {
        var logger = provider.GetRequiredService<ILogger<JwksProvider>>();
        string issuer = Environment.GetEnvironmentVariable("AUTH_NHSLOGIN_ISSUER_URL");
        return new JwksProvider(logger, issuer);
      });

      services.AddSingleton<ITokenService, TokenService>();
      services.AddAuthorization();
    })
    .UseSerilog((context, services, loggerConfiguration) =>
    {
      loggerConfiguration
          .MinimumLevel.Information()
          .Enrich.FromLogContext()
          .Destructure.With(new NhsNumberHashingPolicy()) // Apply NHS number hashing by default
          .Enrich.WithSensitiveDataMasking(options =>
          {
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
