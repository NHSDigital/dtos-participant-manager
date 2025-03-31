using Flagsmith;
using Moq;

namespace ParticipantManager.Shared.Client;

public class FeatureFlagClientTests
{
    private readonly string _featureName = "test_feature";
    private readonly Guid _participantId = Guid.NewGuid();
    private readonly Mock<IFlags> _mockFlags = new();
    private readonly Mock<IFlagsmithClient> _mockFlagsmithClient = new();

    [Fact]
    public async Task IsFeatureEnabledForParticipant_ReturnsTrue_WhenFeatureIsEnabled()
    {
        // Arrange
        _mockFlags.Setup(f => f.IsFeatureEnabled(_featureName)).ReturnsAsync(true);

        _mockFlagsmithClient
            .Setup(c => c.GetIdentityFlags(It.IsAny<string>(), It.IsAny<List<ITrait>>(), false))
            .ReturnsAsync(_mockFlags.Object);

        var featureFlagClient = new FeatureFlagClient(_mockFlagsmithClient.Object);

        // Act
        var result = await featureFlagClient.IsFeatureEnabledForParticipant(_featureName, _participantId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsFeatureEnabledForParticipant_ReturnsFalse_WhenFeatureIsDisabled()
    {
        // Arrange
        _mockFlags.Setup(f => f.IsFeatureEnabled(_featureName)).ReturnsAsync(false);

        _mockFlagsmithClient
            .Setup(c => c.GetIdentityFlags(It.IsAny<string>(), It.IsAny<List<ITrait>>(), false))
            .ReturnsAsync(_mockFlags.Object);

        var featureFlagClient = new FeatureFlagClient(_mockFlagsmithClient.Object);

        // Act
        var result = await featureFlagClient.IsFeatureEnabledForParticipant(_featureName, _participantId);

        // Assert
        Assert.False(result);
    }
}

