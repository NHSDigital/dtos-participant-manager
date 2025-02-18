using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace ParticipantManager.Experience.API.Client;

public class CrudApiClient(ILogger<CrudApiClient> logger, HttpClient httpClient) : ICrudApiClient
{
  public async Task<List<PathwayAssignmentDto>?> GetPathwayAssignmentsAsync(string nhsNumber)
  {
    logger.LogInformation("GetPathwayAssignmentsAsync");
    try
    {
      HttpResponseMessage response = await httpClient.GetAsync($"/api/participants/pathwaytypeassignments?nhsnumber={nhsNumber}");
      response.EnsureSuccessStatusCode();
      return await response.Content.ReadFromJsonAsync<List<PathwayAssignmentDto>>(new JsonSerializerOptions
      {
          PropertyNameCaseInsensitive = true
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "GetPathwayAssignmentsAsync");
      throw;
    }
  }
}

public record PathwayAssignmentDto {
  public string AssignmentId {get; set;}
  public string ScreeningName {get; set;}
}
