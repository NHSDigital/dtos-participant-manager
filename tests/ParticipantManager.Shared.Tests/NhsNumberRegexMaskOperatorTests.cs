using Serilog;
using Serilog.Core;
using Serilog.Enrichers.Sensitive;
using Serilog.Sinks.InMemory;

namespace ParticipantManager.Shared;

public class NhsNumberRegexMaskOperatorTests
{
  private readonly Logger _logger;

  public NhsNumberRegexMaskOperatorTests()
  {
    _logger = new LoggerConfiguration()
      .Enrich.WithSensitiveDataMasking(options =>
      {
        options.MaskingOperators.Add(new NhsNumberRegexMaskOperator());
      })
      .WriteTo.InMemory()
      .CreateLogger();
  }

  [Theory]
  [InlineData("0000000000")]
  [InlineData("9991234567")]
  [InlineData("9999999999")]
  public void NhsNumberRegexMaskOperator_MasksNhsNumbers(string nhsNumber)
  {
    // Act
    _logger.Information("NHS Number: {nhsNumber}", nhsNumber);

    // Assert
    var logEvents = InMemorySink.Instance.LogEvents.ToList();
    var logMessage = logEvents.FirstOrDefault()?.RenderMessage();

    Assert.DoesNotContain(nhsNumber, logMessage);
    Assert.Contains($"***MASKED***", logMessage);
  }

  [Theory]
  [InlineData("999999999")]   // 9 digits
  [InlineData("123456789A")]  // 9 digits plus 1 letter
  [InlineData("1234567890A")] // 10 digits plus 1 letter
  [InlineData("12345678901")] // 11 digits
  public void NhsNumberRegexMaskOperator_DoesNotMaskOtherValues(string otherValue)
  {
    // Act
    _logger.Information("NHS Number: {nhsNumber}", otherValue);

    // Assert
    var logEvents = InMemorySink.Instance.LogEvents.ToList();
    var logMessage = logEvents.FirstOrDefault()?.RenderMessage();

    Assert.Contains(otherValue, logMessage);
    Assert.DoesNotContain($"***MASKED***", logMessage);
  }
}
