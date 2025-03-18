using System.Text.Json.Serialization;

namespace ParticipantManager.Shared.DTOs;

public record PathwayEnrolmentDto
{
  public string EnrolmentId { get; set; }
  public string ScreeningName { get; set; }
  [JsonPropertyName("Participant.NhsNumber")]
  public string NhsNumber { get; set; }
}
