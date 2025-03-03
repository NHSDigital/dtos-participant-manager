namespace ParticipantManager.Shared.Client;

using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

public class CrudApiClient(ILogger<CrudApiClient> logger, HttpClient httpClient) : ICrudApiClient
{
  public async Task CreateEnrollmentAsync(string nhsNumber)
  {
    logger.LogInformation("GetPathwayEnrollmentsAsync");
    try
    {
      var response = await httpClient.GetAsync($"/api/participants/pathwaytypeenrollments?nhsnumber={nhsNumber}");
      response.EnsureSuccessStatusCode();
      await response.Content.ReadFromJsonAsync<List<CreateParticipantDTO>>(new JsonSerializerOptions
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
}
