namespace ParticipantManager.Shared.DTOs;

public class CreatePathwayParticipantDto : ParticipantDto
{
  public Guid PathwayTypeId { get; set; }
  public string? PathwayTypeName { get; set; }
  public string? ScreeningName { get; set; }
}
