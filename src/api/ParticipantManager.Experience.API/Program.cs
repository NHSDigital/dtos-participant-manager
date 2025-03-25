using System.Text.Json;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ParticipantManager.Experience.API.Services;
using ParticipantManager.Shared;
using ParticipantManager.Shared.Client;
using ParticipantManager.Shared.Extensions;

var appInsightsConnectionString =
    Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING") ?? string.Empty;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(worker => { worker.UseMiddleware<CorrelationIdMiddleware>(); })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<FunctionContextAccessor>();
        services.AddOpenTelemetry()
            .ConfigureResource(builder => builder
                .AddService("ParticipantManager.Experience.API"))
            .WithTracing(builder => builder
                .AddSource(nameof(ParticipantManager.Experience.API))
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddAzureMonitorTraceExporter(options => { options.ConnectionString = appInsightsConnectionString; }))
            .WithMetrics(builder => builder
                .AddMeter(nameof(ParticipantManager.Experience.API))
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddAzureMonitorMetricExporter(options =>
                {
                    options.ConnectionString =
                        Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
                }));

        services.AddSingleton(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdHandler>();

        services.AddHttpClient<ICrudApiClient, CrudApiClient>((sp, client) =>
        {
            client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("CRUD_API_URL") ?? string.Empty);
        }).AddHttpMessageHandler<CorrelationIdHandler>();

        services.AddSingleton<IJwksProvider>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<JwksProvider>>();
            var issuer = Environment.GetEnvironmentVariable("AUTH_NHSLOGIN_ISSUER_URL");
            return new JwksProvider(logger, issuer);
        });

        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IFeatureFlagClient, FeatureFlagClient>();
        services.AddAuthorization();
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
