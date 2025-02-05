using Microsoft.AspNetCore.Mvc;
using ParticipantManager.API.Models;

namespace ParticipantManager.Experience.API.Client;

public interface ICrudApiClient
{
  Task<PathwayTypeAssignment?> GetPathwayAssignmentsAsync(string nhsNumber);
}
