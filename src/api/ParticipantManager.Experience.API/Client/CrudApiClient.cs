using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ParticipantManager.Experience.API.DTOs;

namespace ParticipantManager.Experience.API.Client;

public class CrudApiClient(ILogger<CrudApiClient> logger, HttpClient httpClient) : ICrudApiClient
{
  public async Task<List<PathwayAssignmentDTO>?> GetPathwayAssignmentsAsync(string nhsNumber)
  {
    logger.LogInformation("GetPathwayAssignmentsAsync");
    try
    {
      var response = await httpClient.GetAsync($"/api/participants/pathwaytypeassignments?nhsnumber={nhsNumber}");
      response.EnsureSuccessStatusCode();
      return await response.Content.ReadFromJsonAsync<List<PathwayAssignmentDTO>>(new JsonSerializerOptions
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

  public async Task<AssignedPathwayDetailsDTO?> GetPathwayAssignmentByIdAsync(string nhsNumber, string assignmentId)
  {
    logger.LogInformation("GetPathwayAssignmentByIdAsync");
    try
    {
      var response =
        await httpClient.GetAsync(
          $"/api/participants/pathwaytypeassignments/nhsnumber/{nhsNumber}/assignmentid/{assignmentId}");
      response.EnsureSuccessStatusCode();

      return await response.Content.ReadFromJsonAsync<AssignedPathwayDetailsDTO>(new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "GetPathwayAssignmentByIdAsync");
      throw;
    }
  }
}
