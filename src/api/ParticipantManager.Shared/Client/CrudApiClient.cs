using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ParticipantManager.Shared.DTOs;

namespace ParticipantManager.Shared.Client;

public class CrudApiClient(ILogger<CrudApiClient> logger, HttpClient httpClient) : ICrudApiClient
{
  public async Task<List<PathwayEnrolmentDTO>?> GetPathwayEnrolmentsAsync(string nhsNumber)
  {
    logger.LogInformation("GetPathwayEnrolmentsAsync");
    try
    {
      var response = await httpClient.GetAsync($"/api/participants/pathwaytypeenrolments?nhsnumber={nhsNumber}");
      response.EnsureSuccessStatusCode();
      return await response.Content.ReadFromJsonAsync<List<PathwayEnrolmentDTO>>(new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "GetPathwayEnrolmentsAsync");
      throw;
    }
  }

  public async Task<EnroledPathwayDetailsDTO?> GetPathwayEnrolmentByIdAsync(string nhsNumber, string enrolmentId)
  {
    logger.LogInformation("GetPathwayEnrolmentByIdAsync");
    try
    {
      var response =
        await httpClient.GetAsync(
          $"/api/participants/pathwaytypeenrolments/nhsnumber/{nhsNumber}/enrolmentid/{enrolmentId}");
      response.EnsureSuccessStatusCode();

      return await response.Content.ReadFromJsonAsync<EnroledPathwayDetailsDTO>(new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "GetPathwayEnrolmentByIdAsync");
      throw;
    }
  }

  public async Task CreateEnrolmentAsync(CreateParticipantEnrolmentDto participantEnrolmentDto)
  {
    logger.LogInformation("CreateEnrolmentAsync");
    try
    {
      // First create the Participant
      var response = await httpClient.PostAsJsonAsync($"/api/participants", participantEnrolmentDto);
      response.EnsureSuccessStatusCode();

      // Then create the enrolment
      var createEnrolmentResponse = await httpClient.PostAsJsonAsync($"/api/participants/pathwayEnrolment", participantEnrolmentDto);
      createEnrolmentResponse.EnsureSuccessStatusCode();

    }
    catch (Exception ex)
    {
      logger.LogError(ex, "GetPathwayEnrolmentsAsync");
      throw;
    }
  }
}
