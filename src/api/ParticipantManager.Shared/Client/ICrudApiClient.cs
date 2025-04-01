using ParticipantManager.Shared.DTOs;

namespace ParticipantManager.Shared.Client;

public interface ICrudApiClient
{
    Task<List<PathwayTypeEnrolmentDto>> GetPathwayTypeEnrolmentsAsync(Guid participantId);
    Task<EnrolledPathwayDetailsDto?> GetPathwayTypeEnrolmentByIdAsync(Guid participantId, Guid enrolmentId);
    Task<ParticipantDto?> GetParticipantByNhsNumberAsync(string nhsNumber);
    Task<Guid?> CreateParticipantAsync(ParticipantDto participantDto);
    Task<bool> CreatePathwayTypeEnrolmentAsync(CreatePathwayTypeEnrolmentDto pathwayTypeEnrolmentDto);
}
