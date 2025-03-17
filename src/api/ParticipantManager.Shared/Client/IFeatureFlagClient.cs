namespace ParticipantManager.Shared.Client;

public interface IFeatureFlagClient
{
  Task<bool> IsFeatureEnabledForParticipant(string featureName, Guid participantId);
}
