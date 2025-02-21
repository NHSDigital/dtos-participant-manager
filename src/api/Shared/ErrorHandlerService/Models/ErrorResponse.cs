namespace ParticipantManager.API.Models;

using System.Net;

public class ErrorResponse
{
    public required string Title { get; set; }
    public HttpStatusCode? Status { get; set; }
    public required string Message { get; set; }
    public Dictionary<string, string> Headers { get; set; } = [];
    public DateTime Timestamp { get; set; }
}
