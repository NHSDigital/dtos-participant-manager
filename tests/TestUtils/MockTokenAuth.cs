using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace TestUtils
{
    public static class MockTokenAuth
    {
        /// <summary>
        /// Generate a mock authorization bearer token using standard JWT libraries
        /// </summary>
        /// <param name="audience">Token audience (mandatory)</param>
        /// <param name="expiresIn">Token expiration in seconds (default: 3600)</param>
        /// <param name="issuer">Token issuer (default: "test-issuer")</param>
        /// <param name="keyId">Key ID for the token header (default: "test-key-id")</param>
        /// <param name="additionalClaims">Additional claims to be added to the token</param>
        /// <returns>Token information including the bearer token string</returns>
        public static TokenInfo GenerateToken(
            string audience = "test-client-id",
            int expiresIn = 3600,
            string issuer = "test-issuer",
            string keyId = "test-key-id",
            IEnumerable<Claim>? additionalClaims = null)
        {
            // Create a symmetric security key with a consistent key for testing
            // In production, you'd use a secure key management system
            byte[] keyBytes = new byte[32]; // 256 bits for HMAC SHA-256
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }
            var securityKey = new SymmetricSecurityKey(keyBytes);
            securityKey.KeyId = keyId; // Set the kid explicitly on the security key

            // Create signing credentials with key ID
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Set token expiration
            var issuedAt = DateTime.UtcNow;
            var expiresAt = issuedAt.AddSeconds(expiresIn);

            // Create base claims for the token
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Iss, issuer),
                new Claim(JwtRegisteredClaimNames.Aud, audience),
                new Claim(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(issuedAt).ToString(), ClaimValueTypes.Integer64)
            };

            // Add any additional claims provided
            if (additionalClaims != null)
            {
                claims.AddRange(additionalClaims);
            }

            // Create JWT security token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                SigningCredentials = signingCredentials,
                Issuer = issuer,
                Audience = audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // Prepare the payload dictionary for the return value
            var payload = new Dictionary<string, object>
            {
                { "iat", EpochTime.GetIntDate(issuedAt) },
                { "exp", EpochTime.GetIntDate(expiresAt) },
                { "iss", issuer },
                { "aud", audience }
            };

            // Add additional claims to the payload dictionary
            if (additionalClaims != null)
            {
                foreach (var claim in additionalClaims)
                {
                    // Only add if not already present (don't overwrite standard claims)
                    if (!payload.ContainsKey(claim.Type))
                    {
                        payload[claim.Type] = claim.Value;
                    }
                }
            }

            return new TokenInfo
            {
                BearerToken = $"Bearer {tokenString}",
                Token = tokenString,
                Payload = payload,
                ExpiresAt = EpochTime.GetIntDate(expiresAt),
                SigningKey = securityKey // Include the signing key so it can be used for validation
            };
        }

        /// <summary>
        /// Contains token information returned from the token generator
        /// Kept this in this class as it's only used here for testing.
        /// </summary>
        public class TokenInfo
        {
            /// <summary>
            /// Full bearer token string with "Bearer " prefix
            /// </summary>
            public string BearerToken { get; set; }

            /// <summary>
            /// JWT token string without "Bearer " prefix
            /// </summary>
            public string Token { get; set; }

            /// <summary>
            /// Dictionary containing the token payload values
            /// </summary>
            public Dictionary<string, object> Payload { get; set; }

            /// <summary>
            /// Unix timestamp when the token expires
            /// </summary>
            public long ExpiresAt { get; set; }

            /// <summary>
            /// The security key used to sign the token (for validation)
            /// </summary>
            public SecurityKey SigningKey { get; set; }
        }
    }
}
