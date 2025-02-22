namespace ParticipantManager.Experience.API.DTOs;

public class AssignedPathwayDetailsDTO
{
  public Guid AssignmentId { get; set; }
  public DateTime AssignmentDate { get; set; }
  public string Status { get; set; }
  public DateTime? NextActionDate { get; set; }
  public string ScreeningName { get; set; }
  public string PathwayName { get; set; }
  public string InfoUrl => PathwayInfoUrlMapper.GetUrl(PathwayName);
}
