using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ParticipantManager.Experience.API.Client;

public class CrudApiClient(ILogger<CrudApiClient> logger, HttpClient httpClient) : ICrudApiClient
{
  public async Task<HttpResponseMessage?> GetPathwayAssignmentsAsync(string nhsNumber)
  {
    logger.LogInformation("GetPathwayAssignmentsAsync");
    try
    {
      var responseMessage =
        await httpClient.GetAsync(
          "/participants/pathwaytypeassignments?nhsnumber={nhsNumber}");
      return responseMessage;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "GetPathwayAssignmentsAsync");
      throw;
    }
  }
}
