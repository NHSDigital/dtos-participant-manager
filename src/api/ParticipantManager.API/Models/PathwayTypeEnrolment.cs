using System.ComponentModel.DataAnnotations;

namespace ParticipantManager.API.Models;

public class PathwayTypeEnrolment
{
    public Guid EnrolmentId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ParticipantId { get; set; }

    [Required]
    public Guid PathwayTypeId { get; set; }

    public DateOnly EnrolmentDate { get; set; }

    public DateOnly? LapsedDate { get; set; }

    public string Status { get; set; } = "Active";

    public DateOnly? NextActionDate { get; set; }

    public Participant? Participant { get; set; }

    public ICollection<Episode> Episodes { get; set; } = [];

    [Required]
    public required string ScreeningName { get; set; }

    [Required]
    public required string PathwayTypeName { get; set; }
}
