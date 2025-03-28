using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace ParticipantManager.Experience.API.Services;

public class JwksProvider(
    ILogger<JwksProvider> logger,
    string issuer,
    IConfigurationManager<OpenIdConnectConfiguration>? configurationManager = null) : IJwksProvider
{
    private readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager = configurationManager
    ?? new ConfigurationManager<OpenIdConnectConfiguration>($"{issuer}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever(), new HttpDocumentRetriever());
    private readonly ILogger<JwksProvider> _logger = logger;

    public async Task<IEnumerable<SecurityKey>> GetSigningKeysAsync()
    {
        _logger.LogInformation("Fetching JWKS keys from OIDC discovery endpoint...");
        var config = await _configurationManager.GetConfigurationAsync(CancellationToken.None);
        _logger.LogInformation("JWKS keys returned.");
        return config.SigningKeys;
    }
}
