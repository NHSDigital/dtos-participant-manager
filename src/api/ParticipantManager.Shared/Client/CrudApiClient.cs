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

  public async Task<ParticipantDTO?> GetParticipantByNhsNumberAsync(string nhsNumber)
  {
    logger.LogInformation($"Running {nameof(GetParticipantByNhsNumberAsync)}");

    var response = await httpClient.GetAsync($"/api/participants?nhsNumber={nhsNumber}");
    logger.LogInformation("Get Participant with NhsNumber: {@NhsNumber}", new { nhsNumber });

    if (!response.IsSuccessStatusCode)
    {
      logger.LogInformation("Participant with NhsNumber: {@NhsNumber} not found", new { nhsNumber });
      return null;
    }

    var participant = await response.Content.ReadFromJsonAsync<ParticipantDTO>(new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });

    logger.LogInformation("Participant with NhsNumber: {@NhsNumber} found", new { nhsNumber });
    return participant;
  }

  public async Task<Guid?> CreateParticipantAsync(ParticipantDTO participantDto)
  {
    logger.LogInformation($"Running {nameof(CreateParticipantAsync)}");

    try
    {
      var response = await httpClient.PostAsJsonAsync($"/api/participants", participantDto);
      response.EnsureSuccessStatusCode();

      logger.LogInformation("Participant with NhsNumber: {@NhsNumber} created", new { participantDto.NHSNumber });

      var participant = await response.Content.ReadFromJsonAsync<ParticipantDTO>(new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      });

      return participant?.ParticipantId;
    }
    catch (HttpRequestException ex)
    {
      logger.LogError(ex, "Participant with NhsNumber: {@NhsNumber} not created", new { participantDto.NHSNumber });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, $"{nameof(CreateParticipantAsync)} failure in response");
    }

    return null;
  }

  public async Task<bool> CreatePathwayEnrolmentAsync(CreatePathwayEnrolmentDto pathwayEnrolmentDto)
  {
    logger.LogInformation($"Running {nameof(CreatePathwayEnrolmentAsync)}");

    try
    {
      var response = await httpClient.PostAsJsonAsync($"/api/participants/pathwayEnrolment", pathwayEnrolmentDto);
      response.EnsureSuccessStatusCode();
      logger.LogInformation("Enrolment created for Participant: {ParticipantId}, on Pathway: {PathwayName}", pathwayEnrolmentDto.ParticipantId, pathwayEnrolmentDto.PathwayTypeName);
      return true;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to create Enrolment for Participant: {ParticipantId}, on Pathway: {PathwayName}", new { pathwayEnrolmentDto.ParticipantId }, pathwayEnrolmentDto.PathwayTypeName);
      return false;
    }
  }
}
