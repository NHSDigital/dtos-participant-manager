namespace ParticipantManager.Shared.Client;

public interface IFeatureFlagClient
{
  Task<bool> IsFeatureEnabled(string featureName);
}
