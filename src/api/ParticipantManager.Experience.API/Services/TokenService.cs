using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using HttpRequestData = Microsoft.Azure.Functions.Worker.Http.HttpRequestData;

namespace ParticipantManager.Experience.API.Services;

/// <summary>
///   Validates an incoming request and extracts any <see cref="ClaimsPrincipal" /> contained within the bearer token.
/// </summary>
public class TokenService(IJwksProvider jwksProvider, ILogger<TokenService> logger) : ITokenService
{
  private const string AuthHeaderName = "Authorization";
  private const string BearerPrefix = "Bearer ";

  private readonly string _audience = Environment.GetEnvironmentVariable("AUTH_NHSLOGIN_CLIENT_ID") ??
                                      throw new InvalidOperationException(
                                        "AUTH_NHSLOGIN_CLIENT_ID environment variable is missing.");

  private readonly string _issuer = Environment.GetEnvironmentVariable("AUTH_NHSLOGIN_ISSUER_URL") ??
                                    throw new InvalidOperationException(
                                      "AUTH_NHSLOGIN_ISSUER_URL environment variable is missing.");

  public async Task<AccessTokenResult> ValidateToken(HttpRequestData request)
  {
    try
    {
      // Get the token from the header
      logger.LogInformation("Validating token");
      if (request.Headers.TryGetValues(AuthHeaderName, out var authHeaderValues) && authHeaderValues.FirstOrDefault() != null &&
          authHeaderValues.FirstOrDefault()!.StartsWith(BearerPrefix))
      {
        var token = authHeaderValues.FirstOrDefault()?.Replace("Bearer ", "");
        var tokenParams = new TokenValidationParameters
        {
          ValidAudience = _audience,
          ValidateAudience = true,
          ValidIssuer = _issuer,
          ValidateIssuer = true,
          RequireSignedTokens = false,
          ValidateIssuerSigningKey = false,
          ValidateLifetime = false,
          //TODO Make sure this is set to true
          IssuerSigningKeys = await jwksProvider.GetSigningKeysAsync()
        };
        // Validate the token
        var handler = new JwtSecurityTokenHandler();
        logger.LogInformation("About to validate access token");
        var result = handler.ValidateToken(token, tokenParams, out var securityToken);
        var votClaim = result.Claims.FirstOrDefault(c => c.Type == "vot")?.Value;
        if (string.IsNullOrEmpty(votClaim) || !votClaim.StartsWith("P9"))
        {
          throw new SecurityTokenValidationException($"Invalid VOT claim. Expected P9 but was {votClaim}");
        }
        return AccessTokenResult.Success(result);
      }

      logger.LogInformation("No valid token found");
      return AccessTokenResult.NoToken();
    }
    catch (SecurityTokenExpiredException se)
    {
      logger.LogError(se, "Token expired at {SecurityMessage}", se.Message);
      return AccessTokenResult.Expired();
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Token validation exception  at {ExceptionMessage}", ex.Message);
      return AccessTokenResult.Error(ex);
    }
  }
}
