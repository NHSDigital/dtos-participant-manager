namespace ParticipantManager.Shared.DTOs;

public class CreatePathwayParticipantDto : ParticipantDto
{
    public Guid PathwayTypeId { get; set; }
    public required string PathwayTypeName { get; set; }
    public required string ScreeningName { get; set; }
}
