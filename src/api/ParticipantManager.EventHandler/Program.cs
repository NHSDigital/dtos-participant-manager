using Azure.Identity;
using Azure.Messaging.EventGrid;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ParticipantManager.Shared;
using ParticipantManager.Shared.Client;
using ParticipantManager.Shared.Extensions;

var appInsightsConnectionString =
  Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING") ?? string.Empty;

var host = new HostBuilder()
  .ConfigureFunctionsWebApplication()
  .ConfigureServices((context, services) =>
  {
    services.AddHttpContextAccessor();
    services.AddTransient<CorrelationIdHandler>();

    services.AddHttpClient<ICrudApiClient, CrudApiClient>((sp, client) =>
    {
      client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("CRUD_API_URL") ?? string.Empty);
    }).AddHttpMessageHandler<CorrelationIdHandler>();
    services.AddSingleton(sp =>
    {
      if (HostEnvironmentEnvExtensions.IsDevelopment(context.HostingEnvironment))
      {
        var credentials = new Azure.AzureKeyCredential(Environment.GetEnvironmentVariable("EVENT_GRID_TOPIC_KEY"));
        return new EventGridPublisherClient(new Uri(Environment.GetEnvironmentVariable("EVENT_GRID_TOPIC_URL")), credentials);
      }

      return new EventGridPublisherClient(new Uri(Environment.GetEnvironmentVariable("EVENT_GRID_TOPIC_URL")), new ManagedIdentityCredential());
    });
  })
  .ConfigureOpenTelemetry(nameof(ParticipantManager.EventHandler), appInsightsConnectionString)
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
