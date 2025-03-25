using ParticipantManager.Shared.Utils;

namespace ParticipantManager.Shared;

public class EnvironmentVariableHelperTests
{
    private const string TestKey = "TEST_ENV_VAR";

    [Fact]
    public void GetRequired_ReturnsValue_WhenVariableIsSet()
    {
        // Arrange
        var expected = "SomeValue";
        Environment.SetEnvironmentVariable(TestKey, expected);

        // Act
        var result = EnvironmentVariableHelper.GetRequired(TestKey);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetRequired_ThrowsException_WhenVariableIsMissing()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TestKey, null);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            EnvironmentVariableHelper.GetRequired(TestKey));

        Assert.Contains($"Environment variable '{TestKey}' is not set", exception.Message);
    }

    [Fact]
    public void GetRequired_ThrowsException_WhenVariableIsEmpty()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TestKey, string.Empty);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            EnvironmentVariableHelper.GetRequired(TestKey));

        Assert.Contains($"Environment variable '{TestKey}' is not set", exception.Message);
    }
}
