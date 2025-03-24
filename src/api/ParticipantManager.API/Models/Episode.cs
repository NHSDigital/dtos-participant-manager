namespace ParticipantManager.API.Models;

public class Episode
{
  public Guid EpisodeId { get; set; }
  public Guid EnrolmentId { get; set; }
  public required string PathwayVersion { get; set; }
  public required string Status { get; set; }
  public required PathwayTypeEnrolment PathwayTypeEnrolment { get; set; }
  public ICollection<Encounter> Encounters { get; set; } = new List<Encounter>();
}
