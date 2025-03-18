namespace ParticipantManager.Shared.DTOs;

public record PathwayEnrolmentDto
{
  public string EnrolmentId { get; set; }
  public string ScreeningName { get; set; }
  public ParticipantDto Participant { get; set; }
}
