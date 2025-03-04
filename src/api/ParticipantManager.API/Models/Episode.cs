namespace ParticipantManager.API.Models;

public class Episode
{
  public Guid EpisodeId { get; set; }
  public Guid EnrolmentId { get; set; }
  public string PathwayVersion { get; set; }
  public string Status { get; set; }
  public PathwayTypeEnrolment PathwayTypeEnrolment { get; set; }
  public ICollection<Encounter> Encounters { get; set; }
}
