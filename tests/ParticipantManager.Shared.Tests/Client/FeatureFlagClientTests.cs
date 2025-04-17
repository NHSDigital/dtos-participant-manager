using Flagsmith;
using Microsoft.Extensions.Logging;
using Moq;

namespace ParticipantManager.Shared.Client;

public class FeatureFlagClientTests
{
    private readonly string _featureName = "test_feature";
    private readonly Guid _participantId = Guid.NewGuid();
    private readonly Mock<IFlags> _mockFlags = new();
    private readonly Mock<IFlagsmithClient> _mockFlagsmithClient = new();
    private readonly Mock<ILogger<FeatureFlagClient>> _loggerMock = new();

    [Fact]
    public async Task IsFeatureEnabledForParticipant_WhenFeatureIsEnabled_ReturnsTrue()
    {
        // Arrange
        _mockFlags.Setup(f => f.IsFeatureEnabled(_featureName)).ReturnsAsync(true);

        _mockFlagsmithClient
            .Setup(c => c.GetIdentityFlags(It.IsAny<string>(), It.IsAny<List<ITrait>>(), false))
            .ReturnsAsync(_mockFlags.Object);

        var featureFlagClient = new FeatureFlagClient(_mockFlagsmithClient.Object, _loggerMock.Object);

        // Act
        var result = await featureFlagClient.IsFeatureEnabledForParticipant(_featureName, _participantId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsFeatureEnabledForParticipant_WhenFeatureIsDisabled_ReturnsFalse()
    {
        // Arrange
        _mockFlags.Setup(f => f.IsFeatureEnabled(_featureName)).ReturnsAsync(false);

        _mockFlagsmithClient
            .Setup(c => c.GetIdentityFlags(It.IsAny<string>(), It.IsAny<List<ITrait>>(), false))
            .ReturnsAsync(_mockFlags.Object);

        var featureFlagClient = new FeatureFlagClient(_mockFlagsmithClient.Object, _loggerMock.Object);

        // Act
        var result = await featureFlagClient.IsFeatureEnabledForParticipant(_featureName, _participantId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsFeatureEnabledForParticipant_WhenFlagsmithClientThrowsException_ReturnsFalse()
    {
        // Arrange
        _mockFlagsmithClient
            .Setup(c => c.GetIdentityFlags(It.IsAny<string>(), It.IsAny<List<ITrait>>(), false))
            .ThrowsAsync(new Exception("API key expired"));

        var featureFlagClient = new FeatureFlagClient(_mockFlagsmithClient.Object, _loggerMock.Object);

        // Act
        var result = await featureFlagClient.IsFeatureEnabledForParticipant(_featureName, _participantId);

        // Assert
        Assert.False(result);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
    }
}

