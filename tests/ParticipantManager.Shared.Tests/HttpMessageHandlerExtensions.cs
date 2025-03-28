using System.Net;
using System.Net.Http.Json;
using Moq;
using Moq.Protected;

namespace ParticipantManager.Shared;

public static class HttpMessageHandlerExtensions
{
    public static Mock<HttpMessageHandler> SetupRequest<T>(this Mock<HttpMessageHandler> mock, HttpMethod method,
        string url, T responseObject, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
    {
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method && req.RequestUri != null && req.RequestUri.PathAndQuery.Contains(url)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = httpStatusCode,
                Content = JsonContent.Create(responseObject)
            });

        return mock;
    }

    public static Mock<HttpMessageHandler> SetupRequest(this Mock<HttpMessageHandler> mock, HttpMethod method,
        string url, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
    {
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == method && req.RequestUri.AbsolutePath == url),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = httpStatusCode
            });

        return mock;
    }

    public static Mock<HttpMessageHandler> SetupRequestException<TException>(this Mock<HttpMessageHandler> mock, HttpMethod method,
        string url) where TException : Exception, new()
    {
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method && req.RequestUri != null && req.RequestUri.PathAndQuery.Contains(url)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new TException());
        return mock;
    }
}
