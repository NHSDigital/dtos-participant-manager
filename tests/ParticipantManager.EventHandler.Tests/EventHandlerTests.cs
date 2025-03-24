using System.Net;
using Azure;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Moq;
using ParticipantManager.EventHandler.Functions;
using ParticipantManager.Shared.Client;
using ParticipantManager.Shared.DTOs;

namespace ParticipantManager.EventHandler.Tests;

public class CreateEnrolmentHandlerTests
{
  private readonly CreateEnrolmentHandler _handler;
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
  }

  [Fact]
  public async Task Run_WithValidEvent_CreatesNewParticipantWhenParticipantDoesNotExist()
  {
    // Arrange
    var participantId = Guid.NewGuid();
    var pathwayParticipantDto = new CreatePathwayParticipantDto
    {
      NhsNumber = "1234567890",
      PathwayTypeId = Guid.NewGuid(),
      PathwayTypeName = "Test Pathway",
      ScreeningName = "Test Screening"
    };

    var cloudEvent = new CloudEvent("/services/CreateEnrolment", "ParticipantInvited", pathwayParticipantDto, typeof(CreatePathwayParticipantDto));

    _mockCrudApiClient.Setup(c => c.GetParticipantByNhsNumberAsync(pathwayParticipantDto.NhsNumber))
      .ReturnsAsync((ParticipantDto?)null);

    _mockCrudApiClient.Setup(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()))
      .ReturnsAsync(participantId);

    _mockCrudApiClient.Setup(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()))
      .ReturnsAsync(true);

    _mockResponse.Setup(r => r.Status).Returns((int)HttpStatusCode.OK);

    _mockEventGridPublisherClient.Setup(c => c.SendEventAsync(It.IsAny<CloudEvent>(), default))
      .ReturnsAsync(_mockResponse.Object);

    // Act
    await _handler.Run(cloudEvent);

    // Assert
    _mockCrudApiClient.Verify(c => c.GetParticipantByNhsNumberAsync(pathwayParticipantDto.NhsNumber), Times.Once);
    _mockCrudApiClient.Verify(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()), Times.Once);
    _mockCrudApiClient.Verify(c => c.CreatePathwayTypeEnrolmentAsync(It.Is<CreatePathwayTypeEnrolmentDto>(dto =>
      dto.ParticipantId == participantId &&
      dto.PathwayTypeId == pathwayParticipantDto.PathwayTypeId &&
      dto.PathwayTypeName == pathwayParticipantDto.PathwayTypeName &&
      dto.ScreeningName == pathwayParticipantDto.ScreeningName
    )), Times.Once);
    _mockEventGridPublisherClient.Verify(c => c.SendEventAsync(It.IsAny<CloudEvent>(), default), Times.Once);
  }

  [Fact]
  public async Task Run_WithValidEvent_UsesExistingParticipant_WhenParticipantExists()
  {
    // Arrange
    var participantId = Guid.NewGuid();
    var pathwayParticipantDto = new CreatePathwayParticipantDto
    {
      NhsNumber = "1234567890",
      PathwayTypeId = Guid.NewGuid(),
      PathwayTypeName = "Test Pathway",
      ScreeningName = "Test Screening"
    };

    var existingParticipant = new ParticipantDto
    {
      ParticipantId = participantId
    };

    var cloudEvent = new CloudEvent("test-source", "test-event", pathwayParticipantDto, typeof(CreatePathwayParticipantDto));

    _mockCrudApiClient.Setup(c => c.GetParticipantByNhsNumberAsync(pathwayParticipantDto.NhsNumber))
      .ReturnsAsync(existingParticipant);

    _mockCrudApiClient.Setup(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()))
      .ReturnsAsync(true);

    _mockResponse.Setup(r => r.Status).Returns((int)HttpStatusCode.OK);

    _mockEventGridPublisherClient.Setup(c => c.SendEventAsync(It.IsAny<CloudEvent>(), default))
      .ReturnsAsync(_mockResponse.Object);

    // Act
    await _handler.Run(cloudEvent);

    // Assert
    _mockCrudApiClient.Verify(c => c.GetParticipantByNhsNumberAsync(pathwayParticipantDto.NhsNumber), Times.Once);
    _mockCrudApiClient.Verify(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()), Times.Never);
    _mockCrudApiClient.Verify(c => c.CreatePathwayTypeEnrolmentAsync(It.Is<CreatePathwayTypeEnrolmentDto>(dto =>
      dto.ParticipantId == participantId &&
      dto.PathwayTypeId == pathwayParticipantDto.PathwayTypeId &&
      dto.PathwayTypeName == pathwayParticipantDto.PathwayTypeName &&
      dto.ScreeningName == pathwayParticipantDto.ScreeningName
    )), Times.Once);
    _mockEventGridPublisherClient.Verify(c => c.SendEventAsync(It.IsAny<CloudEvent>(), default), Times.Once);
  }

  [Fact]
  public async Task Run_WithInvalidEventData_LogsErrorAndReturns()
  {
    // Arrange
    var cloudEvent = new CloudEvent("test-source", "test-event", "invalid json data");

    // Act
    await _handler.Run(cloudEvent);

    // Assert
    _mockCrudApiClient.Verify(c => c.GetParticipantByNhsNumberAsync(It.IsAny<string>()), Times.Never);
    _mockCrudApiClient.Verify(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()), Times.Never);
    _mockCrudApiClient.Verify(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()),
      Times.Never);
    _mockEventGridPublisherClient.Verify(c => c.SendEventAsync(It.IsAny<CloudEvent>(), default), Times.Never);
  }

  [Fact]
  public async Task Run_WithNullPathwayParticipantDto_ReturnsEarly()
  {
    // Arrange
    var cloudEvent = new CloudEvent("test-source", "test-event", "null");

    // Act
    await _handler.Run(cloudEvent);

    // Assert
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
    var participantId = Guid.NewGuid();
    var pathwayParticipantDto = new CreatePathwayParticipantDto
    {
      NhsNumber = "1234567890",
      PathwayTypeId = Guid.NewGuid(),
      PathwayTypeName = "Test Pathway",
      ScreeningName = "Test Screening"
    };

    var cloudEvent = new CloudEvent("test-source", "test-event", pathwayParticipantDto, typeof(CreatePathwayParticipantDto));

    _mockCrudApiClient.Setup(c => c.GetParticipantByNhsNumberAsync(pathwayParticipantDto.NhsNumber))
      .ReturnsAsync((ParticipantDto?)null);

    _mockCrudApiClient.Setup(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()))
      .ReturnsAsync(participantId);

    _mockCrudApiClient.Setup(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()))
      .ReturnsAsync(false);

    // Act
    await _handler.Run(cloudEvent);

    // Assert
    _mockCrudApiClient.Verify(c => c.GetParticipantByNhsNumberAsync(pathwayParticipantDto.NhsNumber), Times.Once);
    _mockCrudApiClient.Verify(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()), Times.Once);
    _mockCrudApiClient.Verify(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()),
      Times.Once);
    _mockEventGridPublisherClient.Verify(c => c.SendEventAsync(It.IsAny<CloudEvent>(), default), Times.Never);
  }

  [Fact]
  public async Task Run_WhenEventSendingFails_LogsError()
  {
    // Arrange
    var participantId = Guid.NewGuid();
    var pathwayParticipantDto = new CreatePathwayParticipantDto
    {
      NhsNumber = "1234567890",
      PathwayTypeId = Guid.NewGuid(),
      PathwayTypeName = "Test Pathway",
      ScreeningName = "Test Screening"
    };

    var cloudEvent = new CloudEvent("test-source", "test-event", pathwayParticipantDto, typeof(CreatePathwayParticipantDto));

    _mockCrudApiClient.Setup(c => c.GetParticipantByNhsNumberAsync(pathwayParticipantDto.NhsNumber))
      .ReturnsAsync((ParticipantDto?)null);

    _mockCrudApiClient.Setup(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()))
      .ReturnsAsync(participantId);

    _mockCrudApiClient.Setup(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()))
      .ReturnsAsync(true);

    _mockResponse.Setup(r => r.Status).Returns((int)HttpStatusCode.BadRequest);

    _mockEventGridPublisherClient.Setup(c => c.SendEventAsync(It.IsAny<CloudEvent>(), default))
      .ReturnsAsync(_mockResponse.Object);

    // Act
    await _handler.Run(cloudEvent);

    // Assert
    _mockCrudApiClient.Verify(c => c.GetParticipantByNhsNumberAsync(pathwayParticipantDto.NhsNumber), Times.Once);
    _mockCrudApiClient.Verify(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()), Times.Once);
    _mockCrudApiClient.Verify(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()),
      Times.Once);
    _mockEventGridPublisherClient.Verify(c => c.SendEventAsync(It.IsAny<CloudEvent>(), default), Times.Once);
  }
}
