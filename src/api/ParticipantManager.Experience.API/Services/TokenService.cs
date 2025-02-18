namespace ParticipantManager.Experience.API.Services;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Azure.Security.KeyVault.Secrets;

public class TokenService(ILogger<TokenService> logger, SecretClient secretClient) : ITokenService
{
  private readonly string _issuer = Environment.GetEnvironmentVariable("Authentication:Issuer");
  private readonly string _audience = Environment.GetEnvironmentVariable("Authentication:Audience");

  private readonly ILogger<TokenService> _logger = logger;

  /// <summary>
  /// :white_tick: Extracts token from Authorization header
  /// </summary>
  public string? ExtractToken(IEnumerable<string> authorizationHeader)
  {
    var accessToken = authorizationHeader.FirstOrDefault()?.Replace("Bearer ", "");

    if (string.IsNullOrEmpty(accessToken) || !accessToken.StartsWith("Bearer "))
    {
      _logger.LogWarning("Missing or invalid Authorization header.");
      return null;
    }
    return accessToken.Substring("Bearer ".Length).Trim();
  }
  /// <summary>
  /// :white_tick: Validates token and returns ClaimsPrincipal
  /// </summary>
  public ClaimsPrincipal? ValidateToken(string token)
  {
    try
    {
      var _clientSecret = secretClient.GetSecretAsync(Environment.GetEnvironmentVariable("SECRET_NAME")).ToString();
      var tokenHandler = new JwtSecurityTokenHandler();
      var key = Convert.FromBase64String(_clientSecret);
      var validationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = _issuer,
        ValidAudience = _audience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
      };
      SecurityToken validatedToken;
      var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
      return principal;
    }
    catch (Exception ex)
    {
      _logger.LogError("Token validation failed: {Message}", ex.Message);
      return null;
    }
  }
}
