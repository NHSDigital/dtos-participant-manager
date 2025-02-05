using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParticipantManager.API.Models;

namespace ParticipantManager.Experience.API.Client;

public class CrudApiClient(ILogger<CrudApiClient> logger, HttpClient httpClient) : ICrudApiClient
{
  public async Task<PathwayTypeAssignment?> GetPathwayAssignmentsAsync(string nhsNumber)
  {
    logger.LogInformation("GetPathwayAssignmentsAsync");
    try
    {
      var responseMessage =
        await httpClient.GetFromJsonAsync<PathwayTypeAssignment>(
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
