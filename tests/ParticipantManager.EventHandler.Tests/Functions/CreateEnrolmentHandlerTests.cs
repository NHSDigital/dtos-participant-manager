using System.Net;
using System.Text.Json;
using Azure;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Moq;
using ParticipantManager.EventHandler.Functions;
using ParticipantManager.Shared.Client;
using ParticipantManager.Shared.DTOs;

namespace ParticipantManager.EventHandler.Tests.Functions;

public class CreateEnrolmentHandlerTests
{
    private readonly CreateEnrolmentHandler _handler;
    private readonly Guid _participantId;
    private readonly CreatePathwayParticipantDto _pathwayParticipantDto;
    private readonly Mock<ICrudApiClient> _mockCrudApiClient;
    private readonly Mock<EventGridPublisherClient> _mockEventGridPublisherClient;
    private readonly Mock<ILogger<CreateEnrolmentHandler>> _mockLogger;
    private readonly Mock<Response> _mockResponse = new();

    public CreateEnrolmentHandlerTests()
    {
        _mockLogger = new Mock<ILogger<CreateEnrolmentHandler>>();
        _mockCrudApiClient = new Mock<ICrudApiClient>();
        _mockEventGridPublisherClient = new Mock<EventGridPublisherClient>();
        _handler = new CreateEnrolmentHandler(
            _mockLogger.Object,
            _mockCrudApiClient.Object,
            _mockEventGridPublisherClient.Object);

        _participantId = Guid.NewGuid();
        _pathwayParticipantDto = new CreatePathwayParticipantDto
        {
            Name = "John Doe",
            NhsNumber = "1234567890",
            PathwayTypeId = Guid.NewGuid(),
            PathwayTypeName = "Test Pathway",
            ScreeningName = "Test Screening"
        };
    }

    [Fact]
    public async Task Run_WithValidEvent_CreatesNewParticipantWhenParticipantDoesNotExist()
    {
        // Arrange
        var cloudEvent = new CloudEvent("/services/CreateEnrolment", "ParticipantInvited", _pathwayParticipantDto,
            typeof(CreatePathwayParticipantDto));

        _mockCrudApiClient.Setup(c => c.GetParticipantByNhsNumberAsync(_pathwayParticipantDto.NhsNumber))
            .ReturnsAsync((ParticipantDto?)null);

        _mockCrudApiClient.Setup(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()))
            .ReturnsAsync(_participantId);

        _mockCrudApiClient.Setup(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()))
            .ReturnsAsync(true);

        _mockResponse.Setup(r => r.Status).Returns((int)HttpStatusCode.OK);

        _mockEventGridPublisherClient.Setup(c => c.SendEventAsync(It.IsAny<CloudEvent>(), default))
            .ReturnsAsync(_mockResponse.Object);

        // Act
        await _handler.Run(cloudEvent);

        // Assert
        _mockCrudApiClient.Verify(c => c.GetParticipantByNhsNumberAsync(_pathwayParticipantDto.NhsNumber), Times.Once);
        _mockCrudApiClient.Verify(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()), Times.Once);
        _mockCrudApiClient.Verify(c => c.CreatePathwayTypeEnrolmentAsync(It.Is<CreatePathwayTypeEnrolmentDto>(dto =>
            dto.ParticipantId == _participantId &&
            dto.PathwayTypeId == _pathwayParticipantDto.PathwayTypeId &&
            dto.PathwayTypeName == _pathwayParticipantDto.PathwayTypeName &&
            dto.ScreeningName == _pathwayParticipantDto.ScreeningName
        )), Times.Once);
        _mockEventGridPublisherClient.Verify(c => c.SendEventAsync(It.IsAny<CloudEvent>(), default), Times.Once);
    }

    [Fact]
    public async Task Run_WithValidEvent_UsesExistingParticipant_WhenParticipantExists()
    {
        // Arrange
        var existingParticipant = new ParticipantDto
        {
            ParticipantId = _participantId,
            Name = "John Doe",
            NhsNumber = "1243234"
        };

        var cloudEvent = new CloudEvent("test-source", "test-event", _pathwayParticipantDto,
            typeof(CreatePathwayParticipantDto));

        _mockCrudApiClient.Setup(c => c.GetParticipantByNhsNumberAsync(_pathwayParticipantDto.NhsNumber))
            .ReturnsAsync(existingParticipant);

        _mockCrudApiClient.Setup(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()))
            .ReturnsAsync(true);

        _mockResponse.Setup(r => r.Status).Returns((int)HttpStatusCode.OK);

        _mockEventGridPublisherClient.Setup(c => c.SendEventAsync(It.IsAny<CloudEvent>(), default))
            .ReturnsAsync(_mockResponse.Object);

        // Act
        await _handler.Run(cloudEvent);

        // Assert
        _mockCrudApiClient.Verify(c => c.GetParticipantByNhsNumberAsync(_pathwayParticipantDto.NhsNumber), Times.Once);
        _mockCrudApiClient.Verify(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()), Times.Never);
        _mockCrudApiClient.Verify(c => c.CreatePathwayTypeEnrolmentAsync(It.Is<CreatePathwayTypeEnrolmentDto>(dto =>
            dto.ParticipantId == _participantId &&
            dto.PathwayTypeId == _pathwayParticipantDto.PathwayTypeId &&
            dto.PathwayTypeName == _pathwayParticipantDto.PathwayTypeName &&
            dto.ScreeningName == _pathwayParticipantDto.ScreeningName
        )), Times.Once);
        _mockEventGridPublisherClient.Verify(c => c.SendEventAsync(It.IsAny<CloudEvent>(), default), Times.Once);
    }

    [Fact]
    public async Task Run_LogsErrorAndReturns_WhenEventDataNull()
    {
        // Arrange
        var cloudEvent = new CloudEvent("/services/CreateEnrolment", "ParticipantInvited", null,
            typeof(CreatePathwayParticipantDto));
        cloudEvent.Data = null;

        // Act
        await _handler.Run(cloudEvent);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid cloudEvent. cloudEvent.Data is null")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.Once);
        _mockCrudApiClient.Verify(c => c.GetParticipantByNhsNumberAsync(It.IsAny<string>()), Times.Never);
        _mockCrudApiClient.Verify(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()), Times.Never);
        _mockCrudApiClient.Verify(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()),
            Times.Never);
        _mockEventGridPublisherClient.Verify(c => c.SendEventAsync(It.IsAny<CloudEvent>(), default), Times.Never);
    }

    [Fact]
    public async Task Run_LogsErrorAndReturns_WhenEventDataInvalidJson()
    {
        // Arrange
        var cloudEvent = new CloudEvent("test-source", "test-event", "invalid json data");

        // Act
        await _handler.Run(cloudEvent);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unable to deserialize event data to CreateParticipantEnrolmentDto.")),
                It.IsNotNull<JsonException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.Once);
        _mockCrudApiClient.Verify(c => c.GetParticipantByNhsNumberAsync(It.IsAny<string>()), Times.Never);
        _mockCrudApiClient.Verify(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()), Times.Never);
        _mockCrudApiClient.Verify(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()),
            Times.Never);
        _mockEventGridPublisherClient.Verify(c => c.SendEventAsync(It.IsAny<CloudEvent>(), default), Times.Never);
    }

    [Fact]
    public async Task Run_LogsErrorAndReturns_WhenEventDataNullJson()
    {
        // Arrange
        var cloudEvent = new CloudEvent("test-source", "test-event", null, typeof(CreatePathwayParticipantDto));

        // Act
        await _handler.Run(cloudEvent);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deserialized event data was null.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.Once);
        _mockCrudApiClient.Verify(c => c.GetParticipantByNhsNumberAsync(It.IsAny<string>()), Times.Never);
        _mockCrudApiClient.Verify(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()), Times.Never);
        _mockCrudApiClient.Verify(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()),
            Times.Never);
        _mockEventGridPublisherClient.Verify(c => c.SendEventAsync(It.IsAny<CloudEvent>(), default), Times.Never);
    }

    [Fact]
    public async Task Run_WhenPathwayEnrolmentCreationFails_LogsErrorAndDoesNotSendEvent()
    {
        // Arrange
        var cloudEvent = new CloudEvent("test-source", "test-event", _pathwayParticipantDto,
            typeof(CreatePathwayParticipantDto));

        _mockCrudApiClient.Setup(c => c.GetParticipantByNhsNumberAsync(_pathwayParticipantDto.NhsNumber))
            .ReturnsAsync((ParticipantDto?)null);

        _mockCrudApiClient.Setup(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()))
            .ReturnsAsync(_participantId);

        _mockCrudApiClient.Setup(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()))
            .ReturnsAsync(false);

        // Act
        await _handler.Run(cloudEvent);

        // Assert
        _mockCrudApiClient.Verify(c => c.GetParticipantByNhsNumberAsync(_pathwayParticipantDto.NhsNumber), Times.Once);
        _mockCrudApiClient.Verify(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()), Times.Once);
        _mockCrudApiClient.Verify(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()),
            Times.Once);
        _mockEventGridPublisherClient.Verify(c => c.SendEventAsync(It.IsAny<CloudEvent>(), default), Times.Never);
    }

    [Fact]
    public async Task Run_WhenEventSendingFails_LogsError()
    {
        // Arrange
        var cloudEvent = new CloudEvent("test-source", "test-event", _pathwayParticipantDto,
            typeof(CreatePathwayParticipantDto));

        _mockCrudApiClient.Setup(c => c.GetParticipantByNhsNumberAsync(_pathwayParticipantDto.NhsNumber))
            .ReturnsAsync((ParticipantDto?)null);

        _mockCrudApiClient.Setup(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()))
            .ReturnsAsync(_participantId);

        _mockCrudApiClient.Setup(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()))
            .ReturnsAsync(true);

        _mockResponse.Setup(r => r.Status).Returns((int)HttpStatusCode.BadRequest);

        _mockEventGridPublisherClient.Setup(c => c.SendEventAsync(It.IsAny<CloudEvent>(), default))
            .ReturnsAsync(_mockResponse.Object);

        // Act
        await _handler.Run(cloudEvent);

        // Assert
        _mockCrudApiClient.Verify(c => c.GetParticipantByNhsNumberAsync(_pathwayParticipantDto.NhsNumber), Times.Once);
        _mockCrudApiClient.Verify(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()), Times.Once);
        _mockCrudApiClient.Verify(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()),
            Times.Once);
        _mockEventGridPublisherClient.Verify(c => c.SendEventAsync(It.IsAny<CloudEvent>(), default), Times.Once);
    }
}
