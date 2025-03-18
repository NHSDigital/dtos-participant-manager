using System.Net;
using System.Net.Http.Json;
using Moq;
using Moq.Protected;

namespace ParticipantManager.Shared;

public static class HttpMessageHandlerExtensions
{
  public static Mock<HttpMessageHandler> SetupRequest<T>(this Mock<HttpMessageHandler> mock, HttpMethod method, string url, T responseObject)
  {
    mock.Protected()
      .Setup<Task<HttpResponseMessage>>("SendAsync",
        ItExpr.Is<HttpRequestMessage>(req => req.Method == method && req.RequestUri != null && req.RequestUri.PathAndQuery.Contains(url)),
        ItExpr.IsAny<CancellationToken>())
      .ReturnsAsync(new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.OK,
        Content = JsonContent.Create(responseObject)
      });

    return mock;
  }

  public static Mock<HttpMessageHandler> SetupRequest(this Mock<HttpMessageHandler> mock, HttpMethod method, string url)
  {
    mock.Protected()
      .Setup<Task<HttpResponseMessage>>("SendAsync",
        ItExpr.Is<HttpRequestMessage>(req => req.Method == method && req.RequestUri.AbsolutePath == url),
        ItExpr.IsAny<CancellationToken>())
      .ReturnsAsync(new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.OK
      });

    return mock;
  }
}
