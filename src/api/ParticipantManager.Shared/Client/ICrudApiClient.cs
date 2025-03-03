using ParticipantManager.Shared.DTOs;

namespace ParticipantManager.Shared.Client;

public interface ICrudApiClient
{
  Task<List<PathwayEnrollmentDTO>?> GetPathwayEnrollmentsAsync(string nhsNumber);
  Task<EnrolledPathwayDetailsDTO?> GetPathwayEnrollmentByIdAsync(string nhsNumber, string enrollmentId);
  Task CreateEnrollmentAsync(string nhsNumber);
}
