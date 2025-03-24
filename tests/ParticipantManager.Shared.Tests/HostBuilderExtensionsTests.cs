using Microsoft.Extensions.Hosting;
using ParticipantManager.Shared.Extensions;

namespace ParticipantManager.Shared;

public class HostBuilderExtensionsTests
{
  [Fact]
  public void ConfigureSerilogLogging_ShouldNotReturnNull()
  {
    // Arrange
    var appInsightsConnectionString = "InstrumentationKey=value1";
    var hostBuilder = new HostBuilder();

    // Act
    var configuredHostBuilder = hostBuilder.ConfigureSerilogLogging(appInsightsConnectionString);
    configuredHostBuilder.Build();

    // Assert
    Assert.NotNull(configuredHostBuilder);
  }

  [Fact]
  public void ConfigureOpenTelemetry_ShouldNotReturnNull()
  {
    // Arrange
    var appInsightsConnectionString = "InstrumentationKey=value1";
    var hostBuilder = new HostBuilder();

    // Act
    var configuredHostBuilder = hostBuilder.ConfigureOpenTelemetry("test", appInsightsConnectionString);
    configuredHostBuilder.Build();

    // Assert
    Assert.NotNull(configuredHostBuilder);
  }
}
