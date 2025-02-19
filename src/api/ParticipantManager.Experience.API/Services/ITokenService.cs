using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker.Http;

namespace ParticipantManager.Experience.API.Services;

using System.Security.Claims;
public interface ITokenService
{
  Task<AccessTokenResult> ValidateToken(HttpRequestData request);
}
