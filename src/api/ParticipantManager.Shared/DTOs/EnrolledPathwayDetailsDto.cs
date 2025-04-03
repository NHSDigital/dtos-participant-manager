namespace ParticipantManager.Shared.DTOs;

public class EnrolledPathwayDetailsDto
{
    public Guid EnrolmentId { get; set; }
    public DateOnly EnrolmentDate { get; set; }
    public required string Status { get; set; }
    public DateOnly? NextActionDate { get; set; }
    public required string ScreeningName { get; set; }
    public required string PathwayTypeName { get; set; }
    public string InfoUrl => PathwayInfoUrlMapper.GetUrl(PathwayTypeName);
    public required ParticipantDto Participant { get; set; }
}
