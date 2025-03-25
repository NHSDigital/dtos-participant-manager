using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using ParticipantManager.Experience.API.Services;
using ParticipantManager.TestUtils;

namespace ParticipantManager.Experience.API.Tests
{
    public class TokenServiceTests
    {
        private readonly Mock<IJwksProvider> _mockJwksProvider = new();
        private readonly Mock<ILogger<TokenService>> _mockLogger = new();
        private readonly TokenService _tokenService;
        private readonly JwtSecurityTokenHandler _originalHandler = new();
        private string _dummyToken = SetupRequest.CreateTestToken();
        private SecurityToken _securityToken;
        private const string BearerPrefix = "Bearer ";

        public TokenServiceTests()
        {
            Environment.SetEnvironmentVariable("AUTH_NHSLOGIN_CLIENT_ID", "test-client-id");
            Environment.SetEnvironmentVariable("AUTH_NHSLOGIN_ISSUER_URL", "https://test.issuer.com");

            _mockJwksProvider.Setup(p => p.GetSigningKeysAsync()).ReturnsAsync(new List<SecurityKey> { new SymmetricSecurityKey(Guid.NewGuid().ToByteArray()) });

            // Create the service to test
            _tokenService = new TokenService(_mockJwksProvider.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_MissingEnvironmentVariables_ThrowsInvalidOperationException()
        {
            // Arrange
            Environment.SetEnvironmentVariable("AUTH_NHSLOGIN_CLIENT_ID", null);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                new TokenService(_mockJwksProvider.Object, _mockLogger.Object));
            Assert.Contains("AUTH_NHSLOGIN_CLIENT_ID", exception.Message);

            // Restore for other tests
            Environment.SetEnvironmentVariable("AUTH_NHSLOGIN_CLIENT_ID", "test-client-id");

            // Test missing issuer URL
            Environment.SetEnvironmentVariable("AUTH_NHSLOGIN_ISSUER_URL", null);

            // Act & Assert
            exception = Assert.Throws<InvalidOperationException>(() =>
                new TokenService(_mockJwksProvider.Object, _mockLogger.Object));
            Assert.Contains("AUTH_NHSLOGIN_ISSUER_URL", exception.Message);

            // Restore for other tests
            Environment.SetEnvironmentVariable("AUTH_NHSLOGIN_ISSUER_URL", "https://test.issuer.com");
        }

        [Fact]
        public async Task ValidateToken_NoAuthorizationHeader_ReturnsNoToken()
        {
            // Arrange
            var request = SetupRequest.CreateHttpRequest(null);

            // Act
            var result = await _tokenService.ValidateToken(request);

            // Assert
            Assert.NotNull(result);
            var noTokenResult = AccessTokenResult.NoToken();
            Assert.Equal(noTokenResult.GetType(), result.GetType());
            Assert.Null(result.Principal);
        }

        [Fact]
        public async Task ValidateToken_NonBearerAuthHeader_ReturnsNoToken()
        {
            // Arrange
            var request = SetupRequest.CreateHttpRequest("Basic dXNlcjpwYXNzd29yZA==");

            // Act
            var result = await _tokenService.ValidateToken(request);

            // Assert
            Assert.NotNull(result);
            var noTokenResult = AccessTokenResult.NoToken();
            Assert.Equal(noTokenResult.GetType(), result.GetType());
            Assert.Null(result.Principal);
        }

        [Fact]
        public async Task ValidateToken_EmptyBearerToken_ReturnsNoToken()
        {
            // Arrange
            var request = SetupRequest.CreateHttpRequest($"{BearerPrefix}  ");

            // Act
            var result = await _tokenService.ValidateToken(request);

            // Assert
            Assert.NotNull(result);
            var noTokenResult = AccessTokenResult.NoToken();
            Assert.Equal(noTokenResult.GetType(), result.GetType());
            Assert.Null(result.Principal);
        }

        [Fact]
        public async Task ValidateToken_ValidTokenWithP9VotClaim_ReturnsSuccess()
        {
            // Arrange
            var claims = new List<Claim>
    {
        new Claim("vot", "P9.Cp.Cd"),
        new Claim(ClaimTypes.Name, "Test User"),
        new Claim("aud", "test-client-id"),
        new Claim("iss", "https://test.issuer.com")
    };

            // Create token with these claims
            _dummyToken = SetupRequest.CreateTestToken(claims);
            var request = SetupRequest.CreateHttpRequest($"{BearerPrefix}{_dummyToken}");

            // Create a mock handler specifically for this test
            var mockHandler = new Mock<JwtSecurityTokenHandler>();

            // Set up ReadToken to prevent format validation errors
            mockHandler.Setup(h => h.ReadToken(It.IsAny<string>()))
                .Returns(new JwtSecurityToken());

            // Create a claims identity and principal
            var identity = new ClaimsIdentity(claims, "Bearer");
            var principal = new ClaimsPrincipal(identity);

            // Configure the mock handler to return our principal
            mockHandler.Setup(h => h.ValidateToken(
                It.IsAny<string>(),
                It.Is<TokenValidationParameters>(p =>
                    p.ValidAudience == "test-client-id" &&
                    p.ValidateAudience),
                out _securityToken))
                .Returns(principal);

            // Replace the default JWT handler with our mock
            SetupRequest.ReplaceJwtHandler(mockHandler.Object);

            // Act
            var result = await _tokenService.ValidateToken(request);

            // Assert
            Assert.NotNull(result);
            var successResult = AccessTokenResult.Success(new ClaimsPrincipal());
            Assert.Equal(successResult.GetType(), result.GetType());
            Assert.NotNull(result.Principal);
            Assert.Equal("Test User", result.Principal.Identity.Name);

            var votClaim = result.Principal.FindFirst("vot");
            Assert.NotNull(votClaim);
            Assert.Equal("P9.Cp.Cd", votClaim.Value);
        }

        [Fact]
        public async Task ValidateToken_InvalidVotClaimValue_ReturnsError()
        {
            // Arrange
            var request = SetupRequest.CreateHttpRequest($"{BearerPrefix}{_dummyToken}");

            // Create a mock handler specifically for this test
            var mockHandler = new Mock<JwtSecurityTokenHandler>();

            // Set up ReadToken to prevent format validation errors
            mockHandler.Setup(h => h.ReadToken(It.IsAny<string>()))
                .Returns(new JwtSecurityToken());

            // Set up the claims with an invalid VOT value
            var invalidClaims = new List<Claim>
            {
                new Claim("vot", "P1.Cp.Cd"), // Not P9
                new Claim(ClaimTypes.Name, "Test User")
            };

            SecurityToken dummyToken = null;
            mockHandler.Setup(h => h.ValidateToken(
                It.IsAny<string>(),
                It.IsAny<TokenValidationParameters>(),
                out dummyToken))
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(invalidClaims, "Bearer")));

            // Replace the handler before making the call
            SetupRequest.ReplaceJwtHandler(mockHandler.Object);

            // Act
            var result = await _tokenService.ValidateToken(request);

            // Assert
            Assert.NotNull(result);
            var errorResult = AccessTokenResult.Error(new Exception());
            Assert.Equal(errorResult.GetType(), result.GetType());
            Assert.Null(result.Principal);
            Assert.NotNull(result.Exception);
            Assert.IsType<SecurityTokenValidationException>(result.Exception);
            Assert.Contains("Invalid VOT claim", result.Exception.Message);
            Assert.Contains("P1.Cp.Cd", result.Exception.Message);
        }

        [Fact]
        public async Task ValidateToken_MissingVotClaim_ReturnsError()
        {
            // Arrange
            var request = SetupRequest.CreateHttpRequest($"{BearerPrefix}{_dummyToken}");

            // Create a mock handler specifically for this test
            var mockHandler = new Mock<JwtSecurityTokenHandler>();

            // Set up ReadToken to prevent format validation errors
            mockHandler.Setup(h => h.ReadToken(It.IsAny<string>()))
                .Returns(new JwtSecurityToken());

            // Set up claims without a VOT claim
            var claimsWithoutVot = new List<Claim>
            {
                // No VOT claim
                new Claim(ClaimTypes.Name, "Test User")
            };

            SecurityToken dummyToken = null;
            mockHandler.Setup(h => h.ValidateToken(
                It.IsAny<string>(),
                It.IsAny<TokenValidationParameters>(),
                out dummyToken))
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(claimsWithoutVot, "Bearer")));

            // Replace the handler before making the call
            SetupRequest.ReplaceJwtHandler(mockHandler.Object);

            // Act
            var result = await _tokenService.ValidateToken(request);

            // Assert
            Assert.NotNull(result);
            var errorResult = AccessTokenResult.Error(new Exception());
            Assert.Equal(errorResult.GetType(), result.GetType());
            Assert.Null(result.Principal);
            Assert.NotNull(result.Exception);
            Assert.IsType<SecurityTokenValidationException>(result.Exception);
            Assert.Contains("Invalid VOT claim", result.Exception.Message);
            Assert.Contains("null", result.Exception.Message);
        }

        [Fact]
        public async Task ValidateToken_ExpiredToken_ReturnsExpiredResult()
        {
            // Arrange
            var request = SetupRequest.CreateHttpRequest($"{BearerPrefix}{_dummyToken}");

            // Create a mock handler specifically for this test
            var mockHandler = new Mock<JwtSecurityTokenHandler>();

            // Set up ReadToken to prevent format validation errors
            mockHandler.Setup(h => h.ReadToken(It.IsAny<string>()))
                .Returns(new JwtSecurityToken());

            // Set up to throw an expired token exception
            SecurityToken dummyToken = null;
            mockHandler.Setup(h => h.ValidateToken(
                It.IsAny<string>(),
                It.IsAny<TokenValidationParameters>(),
                out dummyToken))
                .Throws(new SecurityTokenExpiredException("Token has expired"));

            // Replace the handler before making the call
            SetupRequest.ReplaceJwtHandler(mockHandler.Object);

            // Act
            var result = await _tokenService.ValidateToken(request);

            // Assert
            Assert.NotNull(result);
            var expiredResult = AccessTokenResult.Expired();
            Assert.Equal(expiredResult.GetType(), result.GetType());
            Assert.Null(result.Principal);
            Assert.NotNull(result.Exception);
            Assert.IsType<SecurityTokenExpiredException>(result.Exception);
        }

        [Fact]
        public async Task ValidateToken_MalformedToken_ReturnsError()
        {
            // Arrange
            var request = SetupRequest.CreateHttpRequest($"{BearerPrefix}{_dummyToken}");

            // Create a mock handler specifically for this test
            var mockHandler = new Mock<JwtSecurityTokenHandler>();

            // Set up ReadToken to prevent format validation errors
            mockHandler.Setup(h => h.ReadToken(It.IsAny<string>()))
                .Returns(new JwtSecurityToken());

            // Set up to throw a malformed token exception
            SecurityToken dummyToken = null;
            mockHandler.Setup(h => h.ValidateToken(
                It.IsAny<string>(),
                It.IsAny<TokenValidationParameters>(),
                out dummyToken))
                .Throws(new SecurityTokenMalformedException("Malformed token"));

            // Replace the handler before making the call
            SetupRequest.ReplaceJwtHandler(mockHandler.Object);

            // Act
            var result = await _tokenService.ValidateToken(request);

            // Assert
            Assert.NotNull(result);
            var errorResult = AccessTokenResult.Error(new Exception());
            Assert.Equal(errorResult.GetType(), result.GetType());
            Assert.Null(result.Principal);
            Assert.NotNull(result.Exception);
            Assert.IsType<SecurityTokenMalformedException>(result.Exception);
        }

        [Fact]
        public async Task ValidateToken_NetworkFailureGettingKeys_HandlesException()
        {
            // Arrange
            var request = SetupRequest.CreateHttpRequest($"{BearerPrefix}{_dummyToken}");

            // Create a mock handler specifically for this test
            var mockHandler = new Mock<JwtSecurityTokenHandler>();

            // Set up ReadToken to prevent format validation errors
            mockHandler.Setup(h => h.ReadToken(It.IsAny<string>()))
                .Returns(new JwtSecurityToken());

            SecurityToken dummyToken = null;
            mockHandler.Setup(h => h.ValidateToken(
                It.IsAny<string>(),
                It.IsAny<TokenValidationParameters>(),
                out dummyToken))
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(), "Bearer")));

            // Replace the handler before making the call
            SetupRequest.ReplaceJwtHandler(mockHandler.Object);

            // Setup JWKS provider to throw an exception
            _mockJwksProvider.Setup(p => p.GetSigningKeysAsync())
                .ThrowsAsync(new InvalidOperationException("Network failure"));

            // Act
            var result = await _tokenService.ValidateToken(request);

            // Assert
            Assert.NotNull(result);
            var errorResult = AccessTokenResult.Error(new Exception());
            Assert.Equal(errorResult.GetType(), result.GetType());
            Assert.Null(result.Principal);
            Assert.NotNull(result.Exception);
            Assert.IsType<InvalidOperationException>(result.Exception);
        }

        [Fact]
        public async Task ValidateToken_InvalidSignature_HandlesException()
        {
            // Arrange
            var request = SetupRequest.CreateHttpRequest($"{BearerPrefix}{_dummyToken}");

            // Create a mock handler specifically for this test
            var mockHandler = new Mock<JwtSecurityTokenHandler>();

            // Set up ReadToken to prevent format validation errors
            mockHandler.Setup(h => h.ReadToken(It.IsAny<string>()))
                .Returns(new JwtSecurityToken());

            // Set up to throw an invalid signature exception
            SecurityToken dummyToken = null;
            mockHandler.Setup(h => h.ValidateToken(
                It.IsAny<string>(),
                It.IsAny<TokenValidationParameters>(),
                out dummyToken))
                .Throws(new SecurityTokenSignatureKeyNotFoundException("Invalid signature"));

            // Replace the handler before making the call
            SetupRequest.ReplaceJwtHandler(mockHandler.Object);

            // Act
            var result = await _tokenService.ValidateToken(request);

            // Assert
            Assert.NotNull(result);
            var errorResult = AccessTokenResult.Error(new Exception());
            Assert.Equal(errorResult.GetType(), result.GetType());
            Assert.Null(result.Principal);
            Assert.NotNull(result.Exception);
            Assert.IsType<SecurityTokenSignatureKeyNotFoundException>(result.Exception);
        }

        [Fact]
        public async Task ValidateToken_InvalidClaims_HandlesException()
        {
            // Arrange
            var request = SetupRequest.CreateHttpRequest($"{BearerPrefix}{_dummyToken}");

            // Create a mock handler specifically for this test
            var mockHandler = new Mock<JwtSecurityTokenHandler>();

            // Set up ReadToken to prevent format validation errors
            mockHandler.Setup(h => h.ReadToken(It.IsAny<string>()))
                .Returns(new JwtSecurityToken());

            // Set up to throw a validation exception for invalid claims
            SecurityToken dummyToken = null;
            mockHandler.Setup(h => h.ValidateToken(
                It.IsAny<string>(),
                It.IsAny<TokenValidationParameters>(),
                out dummyToken))
                .Throws(new SecurityTokenValidationException("Invalid claim"));

            // Replace the handler before making the call
            SetupRequest.ReplaceJwtHandler(mockHandler.Object);

            // Act
            var result = await _tokenService.ValidateToken(request);

            // Assert
            Assert.NotNull(result);
            var errorResult = AccessTokenResult.Error(new Exception());
            Assert.Equal(errorResult.GetType(), result.GetType());
            Assert.Null(result.Principal);
            Assert.NotNull(result.Exception);
            Assert.IsType<SecurityTokenValidationException>(result.Exception);
        }


    }
}
