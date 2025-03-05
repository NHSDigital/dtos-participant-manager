using System.Net;
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
    logger.LogInformation($"Running {nameof(CreateEnrolmentAsync)}");

    try
    {
      ParticipantDTO? participant;

      var getParticipantResponse = await httpClient.GetAsync($"/api/participants?nhsNumber={participantEnrolmentDto.NHSNumber}");
      logger.LogInformation("Get Participant with NhsNumber: {@NhsNumber}", new { participantEnrolmentDto.NHSNumber });

      if (getParticipantResponse.StatusCode != HttpStatusCode.OK)
      {
        logger.LogInformation("Participant with NhsNumber: {@NhsNumber} not found, creating new participant", new { participantEnrolmentDto.NHSNumber });
        var createParticipantResponse = await httpClient.PostAsJsonAsync($"/api/participants", participantEnrolmentDto);
        createParticipantResponse.EnsureSuccessStatusCode();

        participant = await createParticipantResponse.Content.ReadFromJsonAsync<ParticipantDTO>(new JsonSerializerOptions
        {
          PropertyNameCaseInsensitive = true
        });

        logger.LogInformation("Participant created: {@Participant}", new { participant!.ParticipantId, participantEnrolmentDto.NHSNumber });
      }

      participant = await getParticipantResponse.Content.ReadFromJsonAsync<ParticipantDTO>(new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      });

      participantEnrolmentDto.ParticipantId = participant!.ParticipantId;

      var createEnrolmentResponse = await httpClient.PostAsJsonAsync($"/api/participants/pathwayEnrolment", participantEnrolmentDto);
      createEnrolmentResponse.EnsureSuccessStatusCode();
      logger.LogInformation("Enrolment created for Participant: {ParticipantId}, on Pathway: {PathwayName}", participant.ParticipantId, participantEnrolmentDto.PathwayTypeName);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to create Enrolment for NhsNumber: {@NhsNumber}, on Pathway: {PathwayName}", new { participantEnrolmentDto.NHSNumber }, participantEnrolmentDto.PathwayTypeName);
      throw;
    }
  }
}
