namespace ParticipantManager.Shared.DTOs;

public class CreateParticipantEnrollmentDto
{
  public string Name { get; set; }
  public DateTime DOB { get; set; }
  public string NHSNumber { get; set; }
  public Guid PathwayTypeId { get; set; }
  public string PathwayName { get; set; }
}
