using Flagsmith;

namespace ParticipantManager.Shared.Client;

public class FeatureFlagClient(IFlagsmithClient flagsmithClient) : IFeatureFlagClient
{
    public async Task<bool> IsFeatureEnabledForParticipant(string featureName, Guid participantId)
    {
        var identifier = participantId.ToString();
        var traitKey = "participant_id";
        var traitList = new List<ITrait> { new Trait(traitKey, participantId) };

        var flags = await flagsmithClient.GetIdentityFlags(identifier, traitList);

        return await flags.IsFeatureEnabled(featureName);
    }
}
