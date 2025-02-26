using Microsoft.Azure.Functions.Worker.Http;

namespace ParticipantManager.Experience.API.Services;

public interface ITokenService
{
  Task<AccessTokenResult> ValidateToken(HttpRequestData request);
}
