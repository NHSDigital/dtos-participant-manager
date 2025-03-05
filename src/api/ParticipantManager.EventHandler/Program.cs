using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ParticipantManager.Shared;
using ParticipantManager.Shared.Client;
using Serilog;
using Serilog.Enrichers.Sensitive;
using Serilog.Formatting.Compact;

var appInsightsConnectionString =
  Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING") ?? string.Empty;

var host = new HostBuilder()
  .ConfigureFunctionsWebApplication()
  .ConfigureServices((context, services) =>
  {
    // services.AddSingleton<FunctionContextAccessor>();
    // services.AddOpenTelemetry()
    //   .ConfigureResource(builder => builder
    //     .AddService("ParticipantManager.Experience.API"))
    //   .WithTracing(builder => builder
    //     .AddSource(nameof(ParticipantManager.EventHandler))
    //     .AddHttpClientInstrumentation()
    //     .AddAspNetCoreInstrumentation()
    //     .AddAzureMonitorTraceExporter(options => { options.ConnectionString = appInsightsConnectionString; }))
    //   .WithMetrics(builder => builder
    //     .AddMeter(nameof(ParticipantManager.EventHandler))
    //     .AddHttpClientInstrumentation()
    //     .AddAspNetCoreInstrumentation()
    //     .AddAzureMonitorMetricExporter(options =>
    //     {
    //       options.ConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
    //     }));

    // services.AddHttpContextAccessor();
    // services.AddTransient<CorrelationIdHandler>();

    // services.AddHttpClient<ICrudApiClient, CrudApiClient>((sp, client) =>
    // {
    //   client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("CRUD_API_URL") ?? string.Empty);
    // });

    // services.AddSingleton<IJwksProvider>(provider =>
    // {
    //   var logger = provider.GetRequiredService<ILogger<JwksProvider>>();
    //   var issuer = Environment.GetEnvironmentVariable("AUTH_NHSLOGIN_ISSUER_URL");
    //   return new JwksProvider(logger, issuer);
    // });

    // services.AddSingleton<ITokenService, TokenService>();
    // services.AddAuthorization();
    services.AddHttpClient<ICrudApiClient, CrudApiClient>((sp, client) =>
    {
      client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("CRUD_API_URL") ?? string.Empty);
    });
  })
  .UseSerilog((context, services, loggerConfiguration) =>
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
  })
  // .ConfigureLogging(logging =>
  // {
  //   logging.AddOpenTelemetry(options =>
  //   {
  //     options.AddAzureMonitorLogExporter(options => { options.ConnectionString = appInsightsConnectionString; });
  //   });
  // })
  .Build();

await host.RunAsync();
