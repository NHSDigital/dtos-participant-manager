using ParticipantManager.Shared.DTOs;

namespace ParticipantManager.Shared.Client;

public interface ICrudApiClient
{
  Task<List<PathwayEnrolmentDto>?> GetPathwayEnrolmentsAsync(string nhsNumber);
  Task<EnrolledPathwayDetailsDto?> GetPathwayEnrolmentByIdAsync(string nhsNumber, string enrolmentId);
  Task<ParticipantDto?> GetParticipantByNhsNumberAsync(string nhsNumber);
  Task<Guid?> CreateParticipantAsync(ParticipantDto participantDto);
  Task<bool> CreatePathwayTypeEnrolmentAsync(CreatePathwayTypeEnrolmentDto pathwayTypeEnrolmentDto);
}
