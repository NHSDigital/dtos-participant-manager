namespace ParticipantManager.Shared.DTOs;
public class CreatePathwayParticipantDto : ParticipantDTO
{
  public Guid PathwayTypeId { get; set; }
  public string PathwayTypeName { get; set; }
  public string ScreeningName { get; set; }
}
