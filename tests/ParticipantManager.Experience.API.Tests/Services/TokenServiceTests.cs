using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using ParticipantManager.Experience.API.Services;
using ParticipantManager.TestUtils;
using TestUtils;

namespace ParticipantManager.Experience.API.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly Mock<IJwksProvider> _mockJwksProvider = new();
        private readonly Mock<ILogger<TokenService>> _mockLogger = new();
        private readonly TokenService _tokenService;
        private const string TestClientId = "test-client-id";
        private const string TestIssuerUrl = "test-issuer";
        private const string TestKeyId = "test-key-id";

        public TokenServiceTests()
        {
            Environment.SetEnvironmentVariable("AUTH_NHSLOGIN_CLIENT_ID", TestClientId);
            Environment.SetEnvironmentVariable("AUTH_NHSLOGIN_ISSUER_URL", TestIssuerUrl);
            _tokenService = new TokenService(_mockJwksProvider.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ValidateToken_ValidToken_ReturnsSuccessResult()
        {
            // Arrange
            var mockTokenInfo = MockTokenAuth.GenerateToken(
                TestClientId,
                expiresIn: 3600,
                issuer: TestIssuerUrl,
                keyId: TestKeyId,
                additionalClaims: [new Claim("vot", "P9.Cp.Cd")]);

            _mockJwksProvider.Setup(p => p.GetSigningKeysAsync()).ReturnsAsync(new List<SecurityKey> { mockTokenInfo.SigningKey });
            var request = SetupRequest.CreateHttpRequest(mockTokenInfo.BearerToken);

            // Act
            var result = await _tokenService.ValidateToken(request);

            // Assert
            Assert.NotNull(result.Principal);
            Assert.Equal(AccessTokenStatus.Valid, result.Status);
            Assert.Contains(result.Principal.Claims, c => c.Type == "aud" && c.Value == TestClientId);
            Assert.Contains(result.Principal.Claims, c => c.Type == "iss" && c.Value == TestIssuerUrl);
        }

        [Fact]
        public async Task ValidateToken_InvalidToken_ReturnsErrorResult()
        {
            // Arrange
            var mockTokenInfo = MockTokenAuth.GenerateToken(
                TestClientId,
                expiresIn: 3600,
                issuer: TestIssuerUrl,
                keyId: TestKeyId);

            _mockJwksProvider.Setup(p => p.GetSigningKeysAsync()).ReturnsAsync(new List<SecurityKey> { mockTokenInfo.SigningKey });
            var request = SetupRequest.CreateHttpRequest(mockTokenInfo.BearerToken);

            // Act
            var result = await _tokenService.ValidateToken(request);

            // Assert
            Assert.Null(result.Principal);
            Assert.Equal(AccessTokenStatus.Error, result.Status);
            Assert.NotNull(result.Exception);
            Assert.Contains("Invalid VOT claim", result.Exception.Message);
        }

        [Fact]
        public async Task ValidateToken_ExpiredToken_ReturnsExpiredResult()
        {
            // Arrange
            var tokenInfo = MockTokenAuth.GenerateToken(
                TestClientId,
                expiresIn: 3600,
                issuer: TestIssuerUrl,
                keyId: TestKeyId);

            var request = SetupRequest.CreateHttpRequest(tokenInfo.BearerToken);
            _mockJwksProvider.Setup(p => p.GetSigningKeysAsync()).ThrowsAsync(new SecurityTokenExpiredException("Token has expired"));

            // Act
            var result = await _tokenService.ValidateToken(request);

            // Assert
            Assert.Null(result.Principal);
            Assert.Equal(AccessTokenStatus.Expired, result.Status);
        }

        [Fact]
        public async Task ValidateToken_MissingToken_ReturnsNoTokenResult()
        {
            // Arrange
            var request = SetupRequest.CreateHttpRequest("");

            // Act
            var result = await _tokenService.ValidateToken(request);

            // Assert
            Assert.Null(result.Principal);
            Assert.Equal(AccessTokenStatus.NoToken, result.Status);
        }
    }
}
