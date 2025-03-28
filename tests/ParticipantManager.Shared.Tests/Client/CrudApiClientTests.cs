using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using ParticipantManager.API.Models;
using ParticipantManager.Shared.DTOs;

namespace ParticipantManager.Shared.Client;

public class CrudApiClientTests
{
    private readonly Mock<ILogger<CrudApiClient>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly CrudApiClient _client;
    private readonly Guid participantId = Guid.NewGuid();

    public CrudApiClientTests()
    {
        _mockLogger = new Mock<ILogger<CrudApiClient>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://testapi.com")
        };

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        _client = new CrudApiClient(_mockLogger.Object, _httpClient, options);
    }

    [Fact]
    public async Task GetPathwayEnrolmentsAsync_ShouldReturnEnrolments_WhenResponseIsSuccessful()
    {
        // Arrange
        var expectedEnrolments = new List<PathwayTypeEnrolment>()
        {
            new PathwayTypeEnrolment
            {
                EnrolmentDate = DateTime.Now,
                LapsedDate = DateTime.Now,
                Status = "test status",
                NextActionDate = DateTime.Now,
                PathwayTypeId = Guid.NewGuid(),
                ScreeningName = "test screening name",
                PathwayTypeName = "test pathway name",
                Participant = new Participant
                {
                    Name = "test Name",
                    NhsNumber = "1234567890"
                },
                Episodes = []
            }
        };

        _mockHttpMessageHandler.SetupRequest(HttpMethod.Get,
            $"/api/pathwaytypeenrolments?participantId={participantId}",
            expectedEnrolments);

        // Act
        var result = await _client.GetPathwayEnrolmentsAsync(participantId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("test screening name", result[0].ScreeningName);
        Assert.Equal("1234567890", result[0].Participant.NhsNumber);
    }

    [Fact]
    public async Task GetPathwayEnrolmentsAsync_ShouldReturnInvalidOperationException_WhenDeserializationReturnsNull()
    {
        // Arrange
        _mockHttpMessageHandler.SetupRequest<List<PathwayEnrolmentDto>>(HttpMethod.Get,
            $"/api/pathwaytypeenrolments?participantId={participantId}",
            null!);

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _client.GetPathwayEnrolmentsAsync(participantId));

        // Assert
        Assert.NotNull(ex.InnerException);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Contains($"Deserialization returned null for: /api/pathwaytypeenrolments?participantId={participantId}", ex.InnerException.Message);
    }

    [Fact]
    public async Task GetPathwayEnrolmentByIdAsync_ShouldReturnEnrolment_WhenResponseIsSuccessful()
    {
        // Arrange
        var enrolmentId = Guid.NewGuid();
        var expectedEnrolment = new EnrolledPathwayDetailsDto
        {
            EnrolmentId = enrolmentId,
            PathwayTypeName = "test pathway name",
            Participant = new ParticipantDto
            {
                Name = "John Doe",
                NhsNumber = "1234567890"
            },
            Status = "testStatus",
            ScreeningName = "testScreeningName"
        };

        _mockHttpMessageHandler.SetupRequest(HttpMethod.Get,
            $"/api/participants/{participantId}/pathwaytypeenrolments/{enrolmentId}", expectedEnrolment);

        // Act
        var result = await _client.GetPathwayEnrolmentByIdAsync(participantId, enrolmentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(enrolmentId, result.EnrolmentId);
    }

    [Fact]
    public async Task GetParticipantByNhsNumberAsync_ShouldReturnParticipant_WhenFound()
    {
        // Arrange
        var nhsNumber = "1234567890";
        var expectedParticipant = new ParticipantDto { ParticipantId = Guid.NewGuid(), NhsNumber = nhsNumber, Name = "John Doe", };

        _mockHttpMessageHandler.SetupRequest(HttpMethod.Get, $"/api/participants?nhsNumber={nhsNumber}",
            expectedParticipant);

        // Act
        var result = await _client.GetParticipantByNhsNumberAsync(nhsNumber);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(nhsNumber, result.NhsNumber);
    }

    [Fact]
    public async Task CreateParticipantAsync_ShouldReturnParticipantId_WhenSuccessful()
    {
        // Arrange
        var participantDto = new ParticipantDto { NhsNumber = "1234567890", Name = "John Doe", };
        var responseDto = new ParticipantDto { ParticipantId = Guid.NewGuid(), Name = "John Doe", NhsNumber = "1234567890" };

        _mockHttpMessageHandler.SetupRequest(HttpMethod.Post, "/api/participants", responseDto);

        // Act
        var result = await _client.CreateParticipantAsync(participantDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(responseDto.ParticipantId, result);
    }

    [Fact]
    public async Task CreatePathwayTypeEnrolmentAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        var enrolmentDto = new CreatePathwayTypeEnrolmentDto
        {
            ParticipantId = Guid.NewGuid(),
            PathwayTypeName = "Test Pathway",
            ScreeningName = "testScreeningName"
        };

        _mockHttpMessageHandler.SetupRequest(HttpMethod.Post, "/api/pathwaytypeenrolment", enrolmentDto);

        // Act
        var result = await _client.CreatePathwayTypeEnrolmentAsync(enrolmentDto);

        // Assert
        Assert.True(result);
    }
}
