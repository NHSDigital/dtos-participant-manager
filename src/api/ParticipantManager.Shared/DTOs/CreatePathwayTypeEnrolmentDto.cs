namespace ParticipantManager.Shared.DTOs;

public class CreatePathwayTypeEnrolmentDto
{
    public Guid ParticipantId { get; set; }
    public Guid PathwayTypeId { get; set; }
    public required string PathwayTypeName { get; set; }
    public required string ScreeningName { get; set; }
}
