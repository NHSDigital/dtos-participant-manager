namespace ParticipantManager.API.Models;

public class Encounter
{
  public Guid EncounterId { get; set; }
  public Guid EpisodeId { get; set; }
  public DateTime Date { get; set; }
  public string? Outcome { get; set; }
}
