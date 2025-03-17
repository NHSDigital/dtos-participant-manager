namespace ParticipantManager.Shared.DTOs;

public record PathwayEnrolmentDto
{
  public string EnrolmentId { get; set; }
  public string ScreeningName { get; set; }
  public string NhsNumber {get; set;}
}
