namespace ParticipantManager.Experience.API.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

public class JwksProvider(ILogger<JwksProvider> logger, string issuer)
  : IJwksProvider
{
    private readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
      $"{issuer}/.well-known/openid-configuration",
      new OpenIdConnectConfigurationRetriever(),
      new HttpDocumentRetriever());

    public async Task<IEnumerable<SecurityKey>> GetSigningKeysAsync()
    {
        logger.LogInformation("Fetching JWKS keys from OIDC discovery endpoint...");
        var config = await _configurationManager.GetConfigurationAsync(CancellationToken.None);
        logger.LogInformation("JWKS keys returned.");
        return config.SigningKeys;

    }
}
