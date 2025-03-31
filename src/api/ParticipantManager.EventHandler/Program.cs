using System.Text.Json;
using Azure.Identity;
using Azure.Messaging.EventGrid;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ParticipantManager.Shared;
using ParticipantManager.Shared.Client;
using ParticipantManager.Shared.Extensions;

var appInsightsConnectionString = EnvironmentVariables.GetRequired("APPLICATIONINSIGHTS_CONNECTION_STRING");

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdHandler>();

        services.AddSingleton(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        services.AddHttpClient<ICrudApiClient, CrudApiClient>((sp, client) =>
        {
            client.BaseAddress = new Uri(EnvironmentVariables.GetRequired("CRUD_API_URL"));
        }).AddHttpMessageHandler<CorrelationIdHandler>();
        services.AddSingleton(sp =>
        {
            var endpoint = new Uri(EnvironmentVariables.GetRequired("EVENT_GRID_TOPIC_URL"));
            if (context.HostingEnvironment.IsDevelopment())
            {
                var credentials = new Azure.AzureKeyCredential(EnvironmentVariables.GetRequired("EVENT_GRID_TOPIC_KEY"));
                return new EventGridPublisherClient(endpoint, credentials);
            }

            return new EventGridPublisherClient(endpoint, new ManagedIdentityCredential());
        });
    })
    .ConfigureOpenTelemetry(nameof(ParticipantManager.EventHandler), appInsightsConnectionString)
    .ConfigureSerilogLogging(appInsightsConnectionString)
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
