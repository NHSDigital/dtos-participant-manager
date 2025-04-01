using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using ParticipantManager.API.Models;
using ParticipantManager.Shared.DTOs;

namespace ParticipantManager.Shared.Client;

public class CrudApiClientTests
{
    private readonly Mock<ILogger<CrudApiClient>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly CrudApiClient _client;
    private readonly Guid _participantId = Guid.NewGuid();

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
    public async Task GetPathwayTypeEnrolmentsAsync_ShouldReturnEnrolments_WhenResponseIsSuccessful()
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
            $"/api/pathwaytypeenrolments?participantId={_participantId}",
            expectedEnrolments);

        // Act
        var result = await _client.GetPathwayTypeEnrolmentsAsync(_participantId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("test screening name", result[0].ScreeningName);
        Assert.Equal("1234567890", result[0].Participant.NhsNumber);
    }

    [Fact]
    public async Task GetPathwayTypeEnrolmentsAsync_ShouldReturnInvalidOperationException_WhenDeserializationReturnsNull()
    {
        // Arrange
        _mockHttpMessageHandler.SetupRequest<List<PathwayTypeEnrolmentDto>>(HttpMethod.Get,
            $"/api/pathwaytypeenrolments?participantId={_participantId}",
            null!);

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _client.GetPathwayTypeEnrolmentsAsync(_participantId));

        // Assert
        Assert.NotNull(ex.InnerException);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Contains($"Deserialization returned null for: /api/pathwaytypeenrolments?participantId={_participantId}", ex.InnerException.Message);
    }

    [Fact]
    public async Task GetPathwayTypeEnrolmentByIdAsync_ShouldReturnEnrolment_WhenResponseIsSuccessful()
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
            $"/api/participants/{_participantId}/pathwaytypeenrolments/{enrolmentId}", expectedEnrolment);

        // Act
        var result = await _client.GetPathwayTypeEnrolmentByIdAsync(_participantId, enrolmentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(enrolmentId, result.EnrolmentId);
    }

    [Fact]
    public async Task GetPathwayTypeEnrolmentByIdAsync_ShouldThrowInvalidOperationException_WhenEnrolmentNotFound()
    {
        // Arrange
        var enrolmentId = Guid.NewGuid();
        var uri = $"/api/participants/{_participantId}/pathwaytypeenrolments/{enrolmentId}";
        _mockHttpMessageHandler.SetupRequest(HttpMethod.Get, uri, HttpStatusCode.NotFound);

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _client.GetPathwayTypeEnrolmentByIdAsync(_participantId, enrolmentId));

        // Assert
        Assert.NotNull(ex.InnerException);
        Assert.IsType<HttpRequestException>(ex.InnerException);
        Assert.Contains($"Error occurred whilst making request or deserialising object: {uri}", ex.Message);
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
    public async Task GetParticipantByNhsNumberAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var nhsNumber = "1234567890";

        _mockHttpMessageHandler.SetupRequest<ParticipantDto?>(HttpMethod.Get, $"/api/participants?nhsNumber={nhsNumber}",
            null, HttpStatusCode.NotFound);

        // Act
        var result = await _client.GetParticipantByNhsNumberAsync(nhsNumber);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Participant with NhsNumber: ")
                    && v.ToString()!.EndsWith(" not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.Once);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetParticipantByNhsNumberAsync_ShouldReturnNull_WhenExceptionIsThrown()
    {
        // Arrange
        var nhsNumber = "1234567890";

        _mockHttpMessageHandler.SetupRequestException<TaskCanceledException>(HttpMethod.Get, $"/api/participants?nhsNumber={nhsNumber}");

        // Act
        var result = await _client.GetParticipantByNhsNumberAsync(nhsNumber);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.StartsWith($"{nameof(_client.GetParticipantByNhsNumberAsync)} failed to get Participant with")),
                It.IsAny<TaskCanceledException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.Once);
        Assert.Null(result);
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

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task CreateParticipantAsync_ShouldLogErrorAndReturnNull_WhenRequestFails(HttpStatusCode httpStatusCode)
    {
        // Arrange
        var participantDto = new ParticipantDto { NhsNumber = "1234567890", Name = "Test User" };

        _mockHttpMessageHandler.SetupRequest<ParticipantDto?>(HttpMethod.Post, "/api/participants", null!, httpStatusCode);

        // Act
        var result = await _client.CreateParticipantAsync(participantDto);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.StartsWith("Participant with NhsNumber: ")
                    && v.ToString()!.EndsWith(" not created")),
                It.IsAny<HttpRequestException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.Once);
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateParticipantAsync_ShouldLogErrorAndReturnNull_WhenExceptionOccurs()
    {
        // Arrange
        var participantDto = new ParticipantDto { NhsNumber = "1234567890", Name = "Test User" };

        _mockHttpMessageHandler.SetupRequestException<TaskCanceledException>(HttpMethod.Post, "/api/participants");

        // Act
        var result = await _client.CreateParticipantAsync(participantDto);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"{nameof(_client.CreateParticipantAsync)} failure in response")),
                It.IsAny<TaskCanceledException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.Once);
        Assert.Null(result);
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

        _mockHttpMessageHandler.SetupRequest(HttpMethod.Post, "/api/pathwaytypeenrolments", enrolmentDto);

        // Act
        var result = await _client.CreatePathwayTypeEnrolmentAsync(enrolmentDto);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task CreatePathwayTypeEnrolmentAsync_ShouldReturnFalse_WhenRequestFails(HttpStatusCode httpStatusCode)
    {
        // Arrange
        var enrolmentDto = new CreatePathwayTypeEnrolmentDto
        {
            ParticipantId = Guid.NewGuid(),
            PathwayTypeName = "Test Pathway",
            ScreeningName = "Test Screening Name"
        };

        _mockHttpMessageHandler.SetupRequest<PathwayTypeEnrolment>(HttpMethod.Post, "/api/pathwaytypeenrolments", null!, httpStatusCode);

        // Act
        var result = await _client.CreatePathwayTypeEnrolmentAsync(enrolmentDto);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.StartsWith($"Failed to create Enrolment for Participant: {enrolmentDto.ParticipantId}, on Pathway: {enrolmentDto.PathwayTypeName}")),
                It.IsAny<HttpRequestException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.Once);
        Assert.False(result);
    }
}
