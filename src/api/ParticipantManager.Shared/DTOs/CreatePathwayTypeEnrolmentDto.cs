namespace ParticipantManager.Shared.DTOs;

public class CreatePathwayTypeEnrolmentDto
{
  public Guid ParticipantId { get; set; }
  public Guid PathwayTypeId { get; set; }
  public string? PathwayTypeName { get; set; }
  public string? ScreeningName { get; set; }
}
