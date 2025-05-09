namespace ParticipantManager.Shared.DTOs;

public class ParticipantDto
{
    public Guid ParticipantId { get; set; }
    public DateOnly DOB { get; set; }
    public required string NhsNumber { get; set; }
    public required string Name { get; set; }
}
