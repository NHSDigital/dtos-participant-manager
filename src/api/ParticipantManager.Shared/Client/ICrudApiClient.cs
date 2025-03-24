using ParticipantManager.Shared.DTOs;

namespace ParticipantManager.Shared.Client;

public interface ICrudApiClient
{
    Task<List<PathwayEnrolmentDto>?> GetPathwayEnrolmentsAsync(Guid participantId);
    Task<EnrolledPathwayDetailsDto?> GetPathwayEnrolmentByIdAsync(Guid participantId, Guid enrolmentId);
    Task<ParticipantDto?> GetParticipantByNhsNumberAsync(string nhsNumber);
    Task<Guid?> CreateParticipantAsync(ParticipantDto participantDto);
    Task<bool> CreatePathwayTypeEnrolmentAsync(CreatePathwayTypeEnrolmentDto pathwayTypeEnrolmentDto);
}
