namespace ParticipantManager.API.Models;

public class PathwayTypeEnrollment
{
  public Guid EnrollmentId { get; set; }
  public Guid ParticipantId { get; set; }
  public Guid PathwayTypeId { get; set; }
  public DateTime EnrollmentDate { get; set; }
  public DateTime? LapsedDate { get; set; }
  public required string Status { get; set; }
  public DateTime? NextActionDate { get; set; }
  public Participant Participant { get; set; }
  public ICollection<Episode> Episodes { get; set; } = [];
  public required string ScreeningName { get; set; }
  public required string PathwayName { get; set; }
}
