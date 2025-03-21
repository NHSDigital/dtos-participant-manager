using Serilog;
using Serilog.Core;
using Serilog.Sinks.InMemory;

namespace ParticipantManager.Shared;

public class NhsNumberHashingPolicyTests
{
  private readonly Logger _logger;

  public NhsNumberHashingPolicyTests()
  {
    _logger = new LoggerConfiguration()
      .Destructure.With(new NhsNumberHashingPolicy())
      .WriteTo.InMemory()
      .CreateLogger();
  }

  [Theory]
  [InlineData("0000000000")]
  [InlineData("9999999999")]
  public void NhsNumberHashingPolicy_HashesNhsNumbers(string nhsNumber)
  {
    // Arrange
    var participant = new { nhsNumber };
    var hashedNhsNumber = DataMasking.HashNhsNumber(nhsNumber);

    // Act
    _logger.Information("Participant: {@Participant}", participant);

    // Assert
    var logEvents = InMemorySink.Instance.LogEvents.ToList();
    var logMessage = logEvents.FirstOrDefault()?.RenderMessage();

    Assert.DoesNotContain(nhsNumber, logMessage);
    Assert.Contains($"[HASHED:{hashedNhsNumber}]", logMessage);
  }

  [Theory]
  [InlineData("000000000")]   // 9 digits
  [InlineData("123456789A")]  // 9 digits plus 1 letter
  [InlineData("1234567890A")] // 10 digits plus 1 letter
  [InlineData("99999999999")] // 11 digits
  public void NhsNumberHashingPolicy_DoesNotHashOtherValues(string otherValue)
  {
    // Arrange
    var participant = new { otherValue };

    // Act
    _logger.Information("User: {@User}", participant);

    // Assert
    var logEvents = InMemorySink.Instance.LogEvents.ToList();
    var logMessage = logEvents.FirstOrDefault()?.RenderMessage();

    Assert.Contains(otherValue, logMessage);
    Assert.DoesNotContain($"HASHED:", logMessage);
  }
}
