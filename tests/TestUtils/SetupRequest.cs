using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;


namespace ParticipantManager.TestUtils;

public class SetupRequest
{
    private readonly Mock<FunctionContext> _context;

    public SetupRequest()
    {
        _context = new Mock<FunctionContext>();
    }

    /// <summary>
    /// Creates a mock HTTP request with a JSON body
    /// </summary>
    /// <param name="body">The object to serialize as JSON</param>
    /// <returns>A mock HttpRequestData</returns>
    public HttpRequestData CreateMockHttpRequest(object? body)
    {
        var json = JsonSerializer.Serialize(body);
        var byteArray = Encoding.UTF8.GetBytes(json);
        var memoryStream = new MemoryStream(byteArray);
        var mockRequest = new Mock<HttpRequestData>(MockBehavior.Strict, _context.Object);
        mockRequest.Setup(r => r.Body).Returns(memoryStream);
        return mockRequest.Object;
    }

    /// <summary>
    /// Creates a mock HTTP request with query parameters
    /// </summary>
    /// <param name="queryString">The query string (without ?)</param>
    /// <returns>A mock HttpRequestData</returns>
    public HttpRequestData CreateMockHttpRequestWithQuery(string queryString)
    {
        var requestUrl = new Uri($"http://localhost/api/participants?{queryString}");
        var mockRequest = new Mock<HttpRequestData>(MockBehavior.Strict, _context.Object);
        mockRequest.Setup(r => r.Url).Returns(requestUrl);
        mockRequest.Setup(r => r.Headers).Returns(new HttpHeadersCollection());
        return mockRequest.Object;
    }

    public static HttpRequestData CreateHttpRequest(string? authHeader)
    {
        var context = new Mock<FunctionContext>();
        var request = new Mock<HttpRequestData>(MockBehavior.Strict, context.Object);
        var headers = new HttpHeadersCollection(new List<KeyValuePair<string, string>>());
        if (!string.IsNullOrEmpty(authHeader)) headers.Add("Authorization", $"{authHeader}");

        request.Setup(r => r.Headers).Returns(headers);
        return request.Object;
    }

    /// <summary>
    /// Gets the function context object for test usage
    /// </summary>
    public FunctionContext FunctionContext => _context.Object;

    public static void SetupMockHandlerWithClaims(List<Claim> claims)
    {
        var mockHandler = new Mock<JwtSecurityTokenHandler>();
        SecurityToken dummyToken = null;

        mockHandler.Setup(h => h.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out dummyToken)).Returns(new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer")));

        ReplaceJwtHandler(mockHandler.Object);
    }

    public static void ReplaceJwtHandler(JwtSecurityTokenHandler mockHandler)
    {
        var fieldInfo = typeof(JwtSecurityTokenHandler).GetField("_instance",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        fieldInfo?.SetValue(null, mockHandler);
    }

    public static string CreateTestToken(List<Claim> claims = null)
    {
        var claimsDictionary = new Dictionary<string, string>();

        // Add custom claims from the input list
        if (claims != null)
        {
            foreach (var claim in claims)
            {
                claimsDictionary[claim.Type] = claim.Value;
            }
        }

        // Convert claims to base64 encoded payload
        string payload = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes(
                System.Text.Json.JsonSerializer.Serialize(claimsDictionary)
            )
        ).TrimEnd('=')
        .Replace('+', '-')
        .Replace('/', '_');

        // Header: {"alg":"none","typ":"JWT"}
        string header = "eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0";

        // Signature: empty but present
        string signature = "";

        return $"{header}.{payload}.{signature}";
    }
}
