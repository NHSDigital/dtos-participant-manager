using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ParticipantManager.Shared.DTOs;

namespace ParticipantManager.Shared.Client;

public class CrudApiClient(
    ILogger<CrudApiClient> logger,
    HttpClient httpClient,
    JsonSerializerOptions jsonSerializerOptions) : ICrudApiClient
{
    public async Task<List<PathwayTypeEnrolmentDto>> GetPathwayTypeEnrolmentsAsync(Guid participantId)
    {
        logger.LogInformation("GetPathwayTypeEnrolmentsAsync");
        var uri = $"/api/pathwaytypeenrolments?participantId={participantId}";

        try
        {
            var response = await httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<PathwayTypeEnrolmentDto>>(jsonSerializerOptions)
                ?? throw new InvalidOperationException($"Deserialization returned null for: {uri}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetPathwayTypeEnrolmentsAsync");
            throw new InvalidOperationException($"Error occurred whilst making request or deserialising object: {uri}",
                ex);
        }
    }

    public async Task<EnrolledPathwayDetailsDto?> GetPathwayTypeEnrolmentByIdAsync(Guid participantId, Guid enrolmentId)
    {
        logger.LogInformation("GetPathwayTypeEnrolmentByIdAsync");
        var uri = $"/api/participants/{participantId}/pathwaytypeenrolments/{enrolmentId}";

        try
        {
            var response = await httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<EnrolledPathwayDetailsDto>(jsonSerializerOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetPathwayTypeEnrolmentByIdAsync");
            throw new InvalidOperationException($"Error occurred whilst making request or deserialising object: {uri}",
                ex);
        }
    }

    public async Task<ParticipantDto?> GetParticipantByNhsNumberAsync(string nhsNumber)
    {
        logger.LogInformation($"Running {nameof(GetParticipantByNhsNumberAsync)}");
        var uri = $"/api/participants?nhsNumber={nhsNumber}";

        try
        {
            var response = await httpClient.GetAsync(uri);
            logger.LogInformation("Get Participant with NhsNumber: {@NhsNumber}", new { nhsNumber });

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Participant with NhsNumber: {@NhsNumber} not found", new { nhsNumber });
                return null;
            }

            var participant = await response.Content.ReadFromJsonAsync<ParticipantDto>(jsonSerializerOptions)
                ?? throw new InvalidOperationException($"Deserialization returned null for: {uri}");

            logger.LogInformation("Participant with NhsNumber: {@NhsNumber} found", new { nhsNumber });
            return participant;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{FunctionName} failed to get Participant with {@NhsNumber}",
                nameof(GetParticipantByNhsNumberAsync), new { nhsNumber });
            return null;
        }
    }

    public async Task<Guid?> CreateParticipantAsync(ParticipantDto participantDto)
    {
        logger.LogInformation($"Running {nameof(CreateParticipantAsync)}");
        const string uri = "/api/participants";

        try
        {
            var response = await httpClient.PostAsJsonAsync(uri, participantDto);
            response.EnsureSuccessStatusCode();

            logger.LogInformation("Participant with NhsNumber: {@NhsNumber} created", new { participantDto.NhsNumber });

            var participant = await response.Content.ReadFromJsonAsync<ParticipantDto>(jsonSerializerOptions);

            return participant?.ParticipantId;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Participant with NhsNumber: {@NhsNumber} not created",
                new { participantDto.NhsNumber });
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
        const string uri = "/api/pathwaytypeenrolments";

        try
        {
            var response = await httpClient.PostAsJsonAsync(uri, pathwayTypeEnrolmentDto);
            response.EnsureSuccessStatusCode();
            logger.LogInformation("Enrolment created for Participant: {ParticipantId}, on Pathway: {PathwayTypeName}",
                pathwayTypeEnrolmentDto.ParticipantId, pathwayTypeEnrolmentDto.PathwayTypeName);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to create Enrolment for Participant: {ParticipantId}, on Pathway: {PathwayTypeName}",
                pathwayTypeEnrolmentDto.ParticipantId, pathwayTypeEnrolmentDto.PathwayTypeName);
            return false;
        }
    }
}
