using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Moq;
using ParticipantManager.Experience.API.Services;
using System.Security.Cryptography.X509Certificates;

namespace ParticipantManager.Experience.API.Tests
{
    public class JwksProviderTests
    {
        private readonly Mock<IConfigurationManager<OpenIdConnectConfiguration>> _configManagerMock = new();
        private readonly OpenIdConnectConfiguration _mockConfiguration;
        private readonly Mock<ILogger<JwksProvider>> _logger = new();
        private readonly List<SecurityKey> _mockSigningKeys;
        private readonly JwksProvider _jwksProvider;

        public JwksProviderTests()
        {
            var certificate = GenerateSelfSignedCertificate();
            var signingKey = new X509SecurityKey(certificate);
            _mockSigningKeys = new List<SecurityKey> { signingKey };

            _mockConfiguration = new OpenIdConnectConfiguration
            {
                Issuer = "https://example.com",
                JwksUri = "https://example.com/.well-known/jwks"
            };

            _mockConfiguration.SigningKeys.Add(signingKey);
            _configManagerMock.Setup(m => m.GetConfigurationAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_mockConfiguration);
            _jwksProvider = new JwksProvider(_logger.Object, "test", _configManagerMock.Object);
        }

        [Fact]
        public async Task GetSigningKeysAsync_ValidConfiguration_ReturnsExpectedKeys()
        {
            // Act
            var result = await _jwksProvider.GetSigningKeysAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_mockSigningKeys.Count, result.Count());
            Assert.Equal(_mockSigningKeys, result);

            // Verify the mock was called
            _configManagerMock.Verify(m => m.GetConfigurationAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        private static X509Certificate2 GenerateSelfSignedCertificate()
        {
            using var rsa = System.Security.Cryptography.RSA.Create(2048);
            var request = new CertificateRequest("CN=Test Certificate", rsa, System.Security.Cryptography.HashAlgorithmName.SHA256, System.Security.Cryptography.RSASignaturePadding.Pkcs1);
            var certificate = request.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(1));

            return certificate;
        }
    }
}
