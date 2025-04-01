namespace ParticipantManager.Shared.DTOs;

public record PathwayTypeEnrolmentDto
{
    public required string EnrolmentId { get; set; }
    public required string ScreeningName { get; set; }
    public required ParticipantDto Participant { get; set; }
}
