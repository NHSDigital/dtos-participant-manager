using Microsoft.AspNetCore.Mvc;
using ParticipantManager.Experience.API.DTOs;

namespace ParticipantManager.Experience.API.Client;

public interface ICrudApiClient
{
  Task<List<PathwayAssignmentDTO>?> GetPathwayAssignmentsAsync(string nhsNumber);
  Task<AssignedPathwayDetailsDTO?> GetPathwayAssignmentByIdAsync(string nhsNumber, string assignmentId);
}
