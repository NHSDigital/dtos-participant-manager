using System.Text.Json;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ParticipantManager.Experience.API.Services;
using ParticipantManager.Shared;
using ParticipantManager.Shared.Client;
using ParticipantManager.Shared.Extensions;

var appInsightsConnectionString = EnvironmentVariables.GetRequired("APPLICATIONINSIGHTS_CONNECTION_STRING");

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(worker => { worker.UseMiddleware<CorrelationIdMiddleware>(); })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdHandler>();

        services.AddHttpClient<ICrudApiClient, CrudApiClient>((sp, client) =>
        {
            client.BaseAddress = new Uri(EnvironmentVariables.GetRequired("CRUD_API_URL") ?? string.Empty);
        }).AddHttpMessageHandler<CorrelationIdHandler>();

        services.AddSingleton<IJwksProvider>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<JwksProvider>>();
            var issuer = EnvironmentVariables.GetRequired("AUTH_NHSLOGIN_ISSUER_URL");
            return new JwksProvider(logger, issuer);
        });

        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IFeatureFlagClient, FeatureFlagClient>();
        services.AddAuthorization();
    })
    .ConfigureOpenTelemetry(nameof(ParticipantManager.Experience.API), appInsightsConnectionString)
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
