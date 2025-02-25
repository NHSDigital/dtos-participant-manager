using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParticipantManager.Experience.API.Client;
using Microsoft.Extensions.Logging;
using ParticipantManager.Experience.API.Services;
using Serilog;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using ParticipantManager.Experience.API;


var host = new HostBuilder()
  .ConfigureFunctionsWebApplication()
  .ConfigureServices((context, services) =>
  {
    Log.Logger = new LoggerConfiguration()
      .MinimumLevel.Information()
      .Enrich.FromLogContext()
      .Destructure.With(new NhsNumberHashingPolicy()) // Apply NHS number hashing by default
      .WriteTo.Console(new Serilog.Formatting.Compact.RenderedCompactJsonFormatter())
      .WriteTo.ApplicationInsights(
        Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING") ?? "",
        new TraceTelemetryConverter())
      .CreateLogger();
    services.AddLogging(loggingBuilder =>
    {
      loggingBuilder.ClearProviders();
      loggingBuilder.AddSerilog();
    });
  })
  .ConfigureServices(services =>
  {

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

    services.AddSingleton<ITokenService,TokenService>();

    services.AddAuthorization();
  })
  .Build();

host.Run();
