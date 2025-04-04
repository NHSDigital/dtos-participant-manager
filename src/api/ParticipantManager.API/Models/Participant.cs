using System.ComponentModel.DataAnnotations;

namespace ParticipantManager.API.Models;

public class Participant
{
    public Guid ParticipantId { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Date of Birth is required.")]
    public DateTime DOB { get; set; }

    [Required(ErrorMessage = "NHS Number is required.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "NHS Number must be exactly 10 digits.")]
    public required string NhsNumber { get; set; }

    public ICollection<PathwayTypeEnrolment> PathwayTypeEnrolments { get; set; } = new List<PathwayTypeEnrolment>();
}
