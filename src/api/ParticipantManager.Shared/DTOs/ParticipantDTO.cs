namespace ParticipantManager.Shared.DTOs;

public class ParticipantDTO
{
  public Guid ParticipantId { get; set; }
  public DateTime DOB { get; set; }
  public string NHSNumber { get; set; }
  public string Name { get; set; }
}
