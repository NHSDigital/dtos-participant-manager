namespace ParticipantManager.API.Helpers;

using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParticipantManager.API.Models;

public class ErrorHandler(ILogger logger)
{
  private readonly ILogger _logger = logger;
  private static readonly string[] HeaderNames = ["x-correlation-id", "x-request-id", "x-session-id", "x-trace-id"];

  public ObjectResult HandleResponseError(HttpResponseMessage? response, [CallerMemberName] string functionName = "")
  {
    var statusCode = GetStatusCode(response);
    var headers = ExtractHeaders(response);

    LogError(response, headers, functionName);

    return CreateErrorResponse(response, headers, functionName, statusCode);
  }

  private static int GetStatusCode(HttpResponseMessage? response) =>
      (int?)response?.StatusCode ?? StatusCodes.Status500InternalServerError;

  private static Dictionary<string, string> ExtractHeaders(HttpResponseMessage? response) =>
      HeaderNames.ToDictionary(
          name => name,
          name => response?.Headers.TryGetValues(name, out var values) == true
              ? values.FirstOrDefault() ?? "not-found"
              : "not-found"
      );

  private void LogError(HttpResponseMessage? response, Dictionary<string, string> headers, string functionName)
  {
    _logger.LogError(
        "Operation {Operation} failed. Status: {Status}. Headers: {@Headers}. Content: {Content}",
        functionName,
        response?.StatusCode.ToString() ?? "Unknown",
        headers,
        response?.Content.ToString() ?? "No content"
    );
  }

  private static ObjectResult CreateErrorResponse(
      HttpResponseMessage? response,
      Dictionary<string, string> headers,
      string functionName,
      int statusCode)
  {
    var errorResponse = new ErrorResponse
    {
      Title = functionName,
      Status = response?.StatusCode,
      Message = CreateErrorMessage(response),
      Headers = headers,
      Timestamp = DateTime.UtcNow
    };

    return new ObjectResult(errorResponse) { StatusCode = statusCode };
  }

  private static string CreateErrorMessage(HttpResponseMessage? response)
  {
    const string baseMessage = "Failed to retrieve pathway assignments. Status: ";
    var status = response?.StatusCode.ToString() ?? "Unknown";
    var reason = response?.ReasonPhrase ?? "No additional information available";
    return string.Concat(baseMessage, status, ". Reason: ", reason);
  }
}
