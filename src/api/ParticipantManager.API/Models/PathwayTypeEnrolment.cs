using System.ComponentModel.DataAnnotations;

namespace ParticipantManager.API.Models;

public class PathwayTypeEnrolment
{
  public Guid EnrolmentId { get; set; } = Guid.NewGuid();

  [Required]
  public Guid ParticipantId { get; set; }

  [Required]
  public Guid PathwayTypeId { get; set; }

  public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

  public DateTime? LapsedDate { get; set; }

  public string Status { get; set; } = "Active";

  public DateTime? NextActionDate { get; set; }

  public required Participant Participant { get; set; }

  public ICollection<Episode> Episodes { get; set; } = [];

  [Required]
  public required string ScreeningName { get; set; }

  [Required]
  public required string PathwayTypeName { get; set; }

}
