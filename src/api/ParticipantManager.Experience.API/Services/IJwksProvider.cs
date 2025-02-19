using Microsoft.IdentityModel.Tokens;

namespace ParticipantManager.Experience.API.Services;

public interface IJwksProvider
{
  Task<IEnumerable<SecurityKey>> GetSigningKeysAsync();
}
