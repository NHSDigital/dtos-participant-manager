namespace ParticipantManager.Shared.DTOs;

public class ParticipantDto
{
  public Guid ParticipantId { get; set; }
  public DateTime DOB { get; set; }
  public string? NhsNumber { get; set; }
  public string? Name { get; set; }
}
