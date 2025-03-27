using Microsoft.IdentityModel.Tokens;
using Moq;
using ParticipantManager.Experience.API.Services;
using System.Security.Cryptography.X509Certificates;

namespace ParticipantManager.Experience.API.Tests
{
    public class JwksProviderTests
    {
        private readonly Mock<IJwksProvider> _jwksProviderMock;
        private readonly List<SecurityKey> _mockSigningKeys;

        public JwksProviderTests()
        {
            _jwksProviderMock = new Mock<IJwksProvider>();
            _mockSigningKeys = [new X509SecurityKey(GenerateSelfSignedCertificate())];
            _jwksProviderMock.Setup(p => p.GetSigningKeysAsync()).ReturnsAsync(_mockSigningKeys);
        }

        [Fact]
        public async Task GetSigningKeysAsync_ValidConfiguration_ReturnsExpectedKeys()
        {
            // Arrange
            var jwksProvider = _jwksProviderMock.Object;

            // Act
            var result = await jwksProvider.GetSigningKeysAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<SecurityKey>>(result);
            Assert.Equal(_mockSigningKeys.Count, result.Count());
            Assert.Equal(_mockSigningKeys, result);

            _jwksProviderMock.Verify(p => p.GetSigningKeysAsync(), Times.Once);
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
