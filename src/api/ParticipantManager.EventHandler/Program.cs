using System.Text.Json;
using Azure.Identity;
using Azure.Messaging.EventGrid;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ParticipantManager.Shared;
using ParticipantManager.Shared.Client;
using ParticipantManager.Shared.Extensions;

var appInsightsConnectionString = EnvironmentVariables.GetRequired("APPLICATIONINSIGHTS_CONNECTION_STRING");

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(worker => { worker.UseMiddleware<CorrelationIdMiddleware>(); })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<FunctionContextAccessor>();
        services.AddOpenTelemetry()
            .ConfigureResource(builder => builder
                .AddService("ParticipantManager.Experience.API"))
            .WithTracing(builder => builder
                .AddSource(nameof(ParticipantManager.EventHandler))
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddAzureMonitorTraceExporter(options => { options.ConnectionString = appInsightsConnectionString; }))
            .WithMetrics(builder => builder
                .AddMeter(nameof(ParticipantManager.EventHandler))
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddAzureMonitorMetricExporter(options =>
                {
                    options.ConnectionString = appInsightsConnectionString;
                }));

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
