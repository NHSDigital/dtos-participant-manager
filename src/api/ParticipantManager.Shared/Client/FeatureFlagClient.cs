using Flagsmith;

namespace ParticipantManager.Shared.Client;

public class FeatureFlagClient : IFeatureFlagClient
{
    private readonly FlagsmithClient _flagsmithClient;

    public FeatureFlagClient()
    {
        _flagsmithClient = new(Environment.GetEnvironmentVariable("FLAGSMITH_SERVER_SIDE_ENVIRONMENT_KEY"));
    }

    public async Task<bool> IsFeatureEnabledForParticipant(string featureName, Guid participantId)
    {
        var identifier = participantId.ToString();
        var traitKey = "participant_id";
        var traitList = new List<ITrait> { new Trait(traitKey, participantId) };

        var flags = await _flagsmithClient.GetIdentityFlags(identifier, traitList);

        return await flags.IsFeatureEnabled(featureName);
    }
}
