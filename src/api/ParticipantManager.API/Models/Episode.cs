namespace ParticipantManager.API.Models;

public class Episode
{
  public Guid EpisodeId { get; set; }
  public Guid EnrollmentId { get; set; }
  public string PathwayVersion { get; set; }
  public string Status { get; set; }
  public PathwayTypeEnrollment PathwayTypeEnrollment { get; set; }
  public ICollection<Encounter> Encounters { get; set; }
}
