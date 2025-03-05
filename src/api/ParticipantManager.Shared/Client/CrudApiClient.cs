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
    logger.LogInformation("CreateEnrolmentAsync");
    try
    {
      ParticipantDTO participant;
      HttpResponseMessage participantResponse;

      participantResponse = await httpClient.GetAsync($"/api/participants?nhsNumber={participantEnrolmentDto.NHSNumber}");
      logger.LogInformation("Get Participant with NhsNumber: {@NhsNumber}", new { participantEnrolmentDto.NHSNumber });

      if (participantResponse.StatusCode != HttpStatusCode.OK)
      {
        logger.LogInformation("Participant with NhsNumber: {@NhsNumber} not found, new Participant created", participantEnrolmentDto.NHSNumber);
        participantResponse = await httpClient.PostAsJsonAsync($"/api/participants", participantEnrolmentDto);
        participantResponse.EnsureSuccessStatusCode();
        logger.LogInformation("Participant created: {Participant}", new { ParticipantId = participantEnrolmentDto.ParticipantId, NHSNumber = participantEnrolmentDto.NHSNumber });
      }

      participant = await participantResponse.Content.ReadFromJsonAsync<ParticipantDTO>(new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      });

      participantEnrolmentDto.ParticipantId = participant.ParticipantId;

      var createEnrolmentResponse = await httpClient.PostAsJsonAsync($"/api/participants/pathwayEnrolment", participantEnrolmentDto);
      createEnrolmentResponse.EnsureSuccessStatusCode();
      logger.LogInformation("Enrolment created for Participant: {ParticipantId}, on Pathway: {PathwayName}", participant.ParticipantId, participantEnrolmentDto.PathwayTypeName);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to create Enrolment for NhsNumber: {@NhsNumber}, on Pathway: {PathwayName}", participantEnrolmentDto.NHSNumber, participantEnrolmentDto.PathwayTypeName);
      throw;
    }
  }
}
