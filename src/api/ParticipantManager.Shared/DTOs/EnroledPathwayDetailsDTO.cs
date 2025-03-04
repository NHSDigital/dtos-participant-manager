namespace ParticipantManager.Shared.DTOs;

public class EnroledPathwayDetailsDTO
{
  public Guid EnrolmentId { get; set; }
  public DateTime EnrolmentDate { get; set; }
  public string Status { get; set; }
  public DateTime? NextActionDate { get; set; }
  public string ScreeningName { get; set; }
  public string PathwayTypeName { get; set; }
  public string InfoUrl => PathwayInfoUrlMapper.GetUrl(PathwayTypeName);
}
