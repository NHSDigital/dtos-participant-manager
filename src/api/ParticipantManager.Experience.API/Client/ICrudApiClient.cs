using ParticipantManager.Experience.API.DTOs;

namespace ParticipantManager.Experience.API.Client;

public interface ICrudApiClient
{
  Task<List<PathwayEnrollmentDTO>?> GetPathwayEnrollmentsAsync(string nhsNumber);
  Task<EnrolledPathwayDetailsDTO?> GetPathwayEnrollmentByIdAsync(string nhsNumber, string enrollmentId);
  Task CreateEnrollmentAsync(string nhsNumber);
}
