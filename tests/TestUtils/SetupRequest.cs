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
}
