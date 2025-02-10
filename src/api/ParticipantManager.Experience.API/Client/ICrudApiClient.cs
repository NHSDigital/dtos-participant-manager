using Microsoft.AspNetCore.Mvc;

namespace ParticipantManager.Experience.API.Client;

public interface ICrudApiClient
{
  Task<HttpResponseMessage?> GetPathwayAssignmentsAsync(string nhsNumber);
}
