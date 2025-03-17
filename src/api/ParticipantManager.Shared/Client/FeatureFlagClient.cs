using Flagsmith;

namespace ParticipantManager.Shared.Client;

public class FeatureFlagClient : IFeatureFlagClient
{
  private readonly FlagsmithClient _flagsmithClient;

  public FeatureFlagClient()
  {
    _flagsmithClient = new(Environment.GetEnvironmentVariable("FLAGSMITH_SERVER_SIDE_ENVIRONMENT_KEY"));
  }

  public async Task<bool> IsFeatureEnabled(string featureName)
  {
    var flags = await _flagsmithClient.GetEnvironmentFlags();
    return await flags.IsFeatureEnabled(featureName);
  }
}
