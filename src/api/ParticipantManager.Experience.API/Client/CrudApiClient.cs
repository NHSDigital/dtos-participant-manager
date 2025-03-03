using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ParticipantManager.Experience.API.DTOs;

namespace ParticipantManager.Experience.API.Client;

public class CrudApiClient(ILogger<CrudApiClient> logger, HttpClient httpClient) : ICrudApiClient
{
  public async Task<List<PathwayEnrollmentDTO>?> GetPathwayEnrollmentsAsync(string nhsNumber)
  {
    logger.LogInformation("GetPathwayEnrollmentsAsync");
    try
    {
      var response = await httpClient.GetAsync($"/api/participants/pathwaytypeenrollments?nhsnumber={nhsNumber}");
      response.EnsureSuccessStatusCode();
      return await response.Content.ReadFromJsonAsync<List<PathwayEnrollmentDTO>>(new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "GetPathwayEnrollmentsAsync");
      throw;
    }
  }

  public async Task<EnrolledPathwayDetailsDTO?> GetPathwayEnrollmentByIdAsync(string nhsNumber, string enrollmentId)
  {
    logger.LogInformation("GetPathwayEnrollmentByIdAsync");
    try
    {
      var response =
        await httpClient.GetAsync(
          $"/api/participants/pathwaytypeenrollments/nhsnumber/{nhsNumber}/enrollmentid/{enrollmentId}");
      response.EnsureSuccessStatusCode();

      return await response.Content.ReadFromJsonAsync<EnrolledPathwayDetailsDTO>(new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "GetPathwayEnrollmentByIdAsync");
      throw;
    }
  }
}
