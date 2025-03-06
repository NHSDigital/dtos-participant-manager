using ParticipantManager.Shared.DTOs;

namespace ParticipantManager.Shared.Client;

public interface ICrudApiClient
{
  Task<List<PathwayEnrolmentDTO>?> GetPathwayEnrolmentsAsync(string nhsNumber);
  Task<EnrolledPathwayDetailsDTO?> GetPathwayEnrolmentByIdAsync(string nhsNumber, string enrolmentId);
  Task<ParticipantDTO?> GetParticipantByNhsNumberAsync(string nhsNumber);
  Task<Guid?> CreateParticipantAsync(ParticipantDTO participantDto);
  Task CreatePathwayEnrolmentAsync(CreatePathwayEnrolmentDto pathwayEnrolmentDto);
}
