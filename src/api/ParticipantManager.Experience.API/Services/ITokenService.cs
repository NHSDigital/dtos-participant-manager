namespace ParticipantManager.Experience.API.Services;

using System.Security.Claims;
public interface ITokenService
{
  ClaimsPrincipal? ValidateToken(string token);
  string? ExtractToken(IEnumerable<string> authorizationHeader);
}
