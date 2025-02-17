using Microsoft.AspNetCore.Mvc;

namespace ParticipantManager.Experience.API.Client;

public interface ICrudApiClient
{
  Task<List<PathwayAssignmentDto>?> GetPathwayAssignmentsAsync(string nhsNumber);
}
