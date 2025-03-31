using System.Text.Json;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ParticipantManager.API.Data;
using ParticipantManager.Shared;
using ParticipantManager.Shared.Extensions;

var appInsightsConnectionString = EnvironmentVariables.GetRequired("APPLICATIONINSIGHTS_CONNECTION_STRING");
var databaseConnectionString = EnvironmentVariables.GetRequired("ParticipantManagerDatabaseConnectionString");

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        services.AddDbContext<ParticipantManagerDbContext>(options =>
        {
            if (string.IsNullOrEmpty(databaseConnectionString))
                throw new InvalidOperationException("The connection string has not been initialized.");

            options.UseSqlServer(databaseConnectionString);
        });
        services.AddHttpContextAccessor();
    })
    .ConfigureOpenTelemetry(nameof(ParticipantManager.API), appInsightsConnectionString)
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
