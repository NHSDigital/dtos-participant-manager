using Microsoft.Extensions.Logging;

namespace ParticipantManager.Experience.API.Client;

public class CrudApiClient(ILogger<CrudApiClient> logger, HttpClient httpClient) : ICrudApiClient
{
  public async Task<HttpResponseMessage?> GetPathwayAssignmentsAsync(string nhsNumber)
  {
    logger.LogInformation("GetPathwayAssignmentsAsync");
    try
    {
      return await httpClient.GetAsync($"/api/participants/pathwaytypeassignments?nhsnumber={nhsNumber}");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "GetPathwayAssignmentsAsync");
      throw;
    }
  }
}
