using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ParticipantManager.Shared;
using HttpRequestData = Microsoft.Azure.Functions.Worker.Http.HttpRequestData;

namespace ParticipantManager.Experience.API.Services;

/// <summary>
///   Validates an incoming request and extracts any <see cref="ClaimsPrincipal" /> contained within the bearer token.
/// </summary>
public class TokenService(IJwksProvider jwksProvider, ILogger<TokenService> logger) : ITokenService
{
    private const string AuthHeaderName = "Authorization";
    private const string BearerPrefix = "Bearer ";

    private readonly string _audience = EnvironmentVariables.GetRequired("AUTH_NHSLOGIN_CLIENT_ID");

    private readonly string _issuer = EnvironmentVariables.GetRequired("AUTH_NHSLOGIN_ISSUER_URL");

    public static RsaSecurityKey ConvertPemToRsaSecurityKey(string pem)
    {
        // Remove the "-----BEGIN PUBLIC KEY-----" and "-----END PUBLIC KEY-----" parts
        var pemContents = pem.Replace("-----BEGIN PUBLIC KEY-----", string.Empty)
                             .Replace("-----END PUBLIC KEY-----", string.Empty)
                             .Replace("\n", string.Empty)
                             .Replace("\r", string.Empty);

        // Convert the base64 string into bytes
        byte[] keyBytes = Convert.FromBase64String(pemContents);

        // Create an RSA object and import the key
        var rsa = RSA.Create();  // Use RSA.Create() instead of RSACryptoServiceProvider
        rsa.ImportSubjectPublicKeyInfo(keyBytes, out _); // Import the public key

        // Return the RsaSecurityKey
        return new RsaSecurityKey(rsa);
    }

    public async Task<AccessTokenResult> ValidateToken(HttpRequestData request)
    {
        try
        {
            // Get the token from the header
            logger.LogInformation("Validating token");
            if (request.Headers.TryGetValues(AuthHeaderName, out var authHeaderValues) &&
                authHeaderValues.FirstOrDefault() != null &&
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
                    ValidateLifetime = true,
                    IssuerSigningKeys = await jwksProvider.GetSigningKeysAsync()
                };

                if (EnvironmentVariables.GetRequired("NEXT_PUBLIC_API_MOCKING") == "enabled")
                {
                    tokenParams.IssuerSigningKey = ConvertPemToRsaSecurityKey(EnvironmentVariables.GetRequired("TEST_JWT_PUBLIC_KEY"));
                }

                // Validate the token
                var handler = new JwtSecurityTokenHandler();
                logger.LogInformation("About to validate access token");
                var result = handler.ValidateToken(token, tokenParams, out var _);
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
