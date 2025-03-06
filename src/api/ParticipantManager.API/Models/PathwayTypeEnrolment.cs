namespace ParticipantManager.API.Models;

public class PathwayTypeEnrolment
{
  public Guid EnrolmentId { get; set; } = Guid.NewGuid();
  public Guid ParticipantId { get; set; }
  public Guid PathwayTypeId { get; set; }
  public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
  public DateTime? LapsedDate { get; set; }
  public string Status { get; set; } = "Active";
  public DateTime? NextActionDate { get; set; }
  public Participant Participant { get; set; }
  public ICollection<Episode> Episodes { get; set; } = [];
  public required string ScreeningName { get; set; }
  public required string PathwayTypeName { get; set; }
}
