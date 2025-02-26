using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ParticipantManager.API.Data;
using Serilog;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Serilog.Enrichers.Sensitive;
using ParticipantManager.Shared;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
Log.Logger = new LoggerConfiguration()
  .MinimumLevel.Information()
  .Enrich.FromLogContext()
  .Destructure.With(new NhsNumberHashingPolicy()) // Apply NHS number hashing by default
  .Enrich.WithSensitiveDataMasking(options =>
  {
    options.MaskingOperators.Add(new NhsNumberRegexMaskOperator());
    options.MaskingOperators.Add(new EmailAddressMaskingOperator());
  })
  .WriteTo.Console(new Serilog.Formatting.Compact.RenderedCompactJsonFormatter())
  .WriteTo.ApplicationInsights(
    Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING") ?? "",
    new TraceTelemetryConverter())
  .CreateLogger();
builder.Services.AddLogging(loggingBuilder =>
{
  loggingBuilder.ClearProviders();
  loggingBuilder.AddSerilog();
});


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
