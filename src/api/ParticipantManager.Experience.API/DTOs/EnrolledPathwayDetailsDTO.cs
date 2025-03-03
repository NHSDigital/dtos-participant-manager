namespace ParticipantManager.Experience.API.DTOs;

public class EnrolledPathwayDetailsDTO
{
  public Guid EnrollmentId { get; set; }
  public DateTime EnrollmentDate { get; set; }
  public string Status { get; set; }
  public DateTime? NextActionDate { get; set; }
  public string ScreeningName { get; set; }
  public string PathwayName { get; set; }
  public string InfoUrl => PathwayInfoUrlMapper.GetUrl(PathwayName);
}
