namespace ParticipantManager.API.Models;

public class PathwayTypeEnrolment
{
  public Guid EnrolmentId { get; set; }
  public Guid ParticipantId { get; set; }
  public Guid PathwayTypeId { get; set; }
  public DateTime EnrolmentDate { get; set; }
  public DateTime? LapsedDate { get; set; }
  public required string Status { get; set; }
  public DateTime? NextActionDate { get; set; }
  public Participant Participant { get; set; }
  public ICollection<Episode> Episodes { get; set; } = [];
  public required string ScreeningName { get; set; }
  public required string PathwayTypeName { get; set; }
}
