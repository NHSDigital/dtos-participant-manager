using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ParticipantManager.Shared.DTOs;

namespace ParticipantManager.Shared.Client;

public class CrudApiClient(ILogger<CrudApiClient> logger, HttpClient httpClient) : ICrudApiClient
{
  public async Task<List<PathwayEnrolmentDto>?> GetPathwayEnrolmentsAsync(string nhsNumber)
  {
    logger.LogInformation("GetPathwayEnrolmentsAsync");
    var url = $"/api/pathwaytypeenrolments?nhsnumber={nhsNumber}";

    try
    {
      var response = await httpClient.GetAsync(url);
      response.EnsureSuccessStatusCode();
      return await response.Content.ReadFromJsonAsync<List<PathwayEnrolmentDto>>(new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "GetPathwayEnrolmentsAsync");
      throw new InvalidOperationException($"Error occurred whilst making request or deserialising object: {url}", ex);
    }
  }

  public async Task<EnrolledPathwayDetailsDto?> GetPathwayEnrolmentByIdAsync(string nhsNumber, string enrolmentId)
  {
    logger.LogInformation("GetPathwayEnrolmentByIdAsync");
    var url = $"/api/pathwaytypeenrolments/{enrolmentId}";

    try
    {
      var response = await httpClient.GetAsync(url);
      response.EnsureSuccessStatusCode();

      return await response.Content.ReadFromJsonAsync<EnrolledPathwayDetailsDto>(new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "GetPathwayEnrolmentByIdAsync");
      throw new InvalidOperationException($"Error occurred whilst making request or deserialising object: {url}", ex);
    }
  }

  public async Task<ParticipantDto?> GetParticipantByNhsNumberAsync(string nhsNumber)
  {
    logger.LogInformation($"Running {nameof(GetParticipantByNhsNumberAsync)}");
    try
    {
      var response = await httpClient.GetAsync($"/api/participants?nhsNumber={nhsNumber}");
      logger.LogInformation("Get Participant with NhsNumber: {@NhsNumber}", new { nhsNumber });

      if (!response.IsSuccessStatusCode)
      {
        logger.LogError("Participant with NhsNumber: {@NhsNumber} not found", new { nhsNumber });
        return null;
      }

      var participant = await response.Content.ReadFromJsonAsync<ParticipantDto>(new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      });

      logger.LogInformation("Participant with NhsNumber: {@NhsNumber} found", new { nhsNumber });
      return participant;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{FunctionName} failed to get Participant with {@NhsNumber}", nameof(GetParticipantByNhsNumberAsync), new { nhsNumber });
      return null;
    }
  }

  public async Task<Guid?> CreateParticipantAsync(ParticipantDto participantDto)
  {
    logger.LogInformation($"Running {nameof(CreateParticipantAsync)}");

    try
    {
      var response = await httpClient.PostAsJsonAsync($"/api/participants", participantDto);
      response.EnsureSuccessStatusCode();

      logger.LogInformation("Participant with NhsNumber: {@NhsNumber} created", new { participantDto.NHSNumber });

      var participant = await response.Content.ReadFromJsonAsync<ParticipantDto>(new JsonSerializerOptions
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

  public async Task<bool> CreatePathwayTypeEnrolmentAsync(CreatePathwayTypeEnrolmentDto pathwayTypeEnrolmentDto)
  {
    logger.LogInformation($"Running {nameof(CreatePathwayTypeEnrolmentAsync)}");

    try
    {
      var response = await httpClient.PostAsJsonAsync($"/api/pathwaytypeenrolment", pathwayTypeEnrolmentDto);
      response.EnsureSuccessStatusCode();
      logger.LogInformation("Enrolment created for Participant: {ParticipantId}, on Pathway: {PathwayName}", pathwayTypeEnrolmentDto.ParticipantId, pathwayTypeEnrolmentDto.PathwayTypeName);
      return true;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to create Enrolment for Participant: {ParticipantId}, on Pathway: {PathwayName}", new { pathwayTypeEnrolmentDto.ParticipantId }, pathwayTypeEnrolmentDto.PathwayTypeName);
      return false;
    }
  }
}
