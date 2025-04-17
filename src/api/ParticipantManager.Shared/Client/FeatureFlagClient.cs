using Flagsmith;
using Microsoft.Extensions.Logging;

namespace ParticipantManager.Shared.Client;

public class FeatureFlagClient(IFlagsmithClient flagsmithClient, ILogger<FeatureFlagClient> logger) : IFeatureFlagClient
{
    private const bool DefaultValue = false;
    public async Task<bool> IsFeatureEnabledForParticipant(string featureName, Guid participantId)
    {
        try
        {
            var identifier = participantId.ToString();
            var traitKey = "participant_id";
            var traitList = new List<ITrait> { new Trait(traitKey, participantId) };
            var flags = await flagsmithClient.GetIdentityFlags(identifier, traitList);

            return await flags.IsFeatureEnabled(featureName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch feature flag {FeatureName} for participant {ParticipantId}. Using default value {DefaultValue}.", featureName, participantId, DefaultValue);
            return DefaultValue;
        }
    }
}
