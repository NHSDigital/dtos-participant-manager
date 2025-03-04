using ParticipantManager.Shared.DTOs;

namespace ParticipantManager.Shared.Client;

public interface ICrudApiClient
{
  Task<List<PathwayEnrolmentDTO>?> GetPathwayEnrolmentsAsync(string nhsNumber);
  Task<EnroledPathwayDetailsDTO?> GetPathwayEnrolmentByIdAsync(string nhsNumber, string enrolmentId);
  Task CreateEnrolmentAsync(CreateParticipantEnrolmentDto participantDto);
}
