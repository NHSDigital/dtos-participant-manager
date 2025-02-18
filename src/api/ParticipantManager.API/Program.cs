using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ParticipantManager.API.Data;


var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.Services.AddDbContext<ParticipantManagerDbContext>(options =>
{
  var connectionString = Environment.GetEnvironmentVariable("ParticipantManagerDatabaseConnectionString");
  if (string.IsNullOrEmpty(connectionString))
  {
    throw new InvalidOperationException("The connection string has not been initialized.");
  }

  options.UseSqlServer(connectionString);
});

builder.Services.AddLogging(builder =>
{
  builder.AddConsole(); // Use console logging
  builder.SetMinimumLevel(LogLevel.Debug);
});

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
