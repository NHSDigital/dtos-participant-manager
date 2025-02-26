namespace ParticipantManager.API.Models;

public class PathwayTypeAssignment
{
  public Guid AssignmentId { get; set; }
  public Guid ParticipantId { get; set; }
  public Guid PathwayId { get; set; }
  public DateTime AssignmentDate { get; set; }
  public DateTime? LapsedDate { get; set; }
  public string Status { get; set; }
  public DateTime? NextActionDate { get; set; }
  public Participant Participant { get; set; }
  public Guid PathwayTypeId { get; set; }
  public ICollection<Episode> Episodes { get; set; }
  public string ScreeningName { get; set; }
  public string PathwayName { get; set; }
}
