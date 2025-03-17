using System.Net;
using Azure;
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
      NHSNumber = "1234567890",
      PathwayTypeId = Guid.NewGuid(),
      PathwayTypeName = "Test Pathway",
      ScreeningName = "Test Screening"
    };

    var eventGridEvent = new EventGridEvent("/participants/12345", "ParticipantInvited", "0.1", pathwayParticipantDto,
      typeof(CreatePathwayParticipantDto));

    _mockCrudApiClient.Setup(c => c.GetParticipantByNhsNumberAsync(pathwayParticipantDto.NHSNumber))
      .ReturnsAsync((ParticipantDto)null);

    _mockCrudApiClient.Setup(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()))
      .ReturnsAsync(participantId);

    _mockCrudApiClient.Setup(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()))
      .ReturnsAsync(true);

    _mockResponse.Setup(r => r.Status).Returns((int)HttpStatusCode.OK);

    _mockEventGridPublisherClient.Setup(c => c.SendEventAsync(It.IsAny<EventGridEvent>(), default))
      .ReturnsAsync(_mockResponse.Object);

    // Act
    await _handler.Run(eventGridEvent);

    // Assert
    _mockCrudApiClient.Verify(c => c.GetParticipantByNhsNumberAsync(pathwayParticipantDto.NHSNumber), Times.Once);
    _mockCrudApiClient.Verify(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()), Times.Once);
    _mockCrudApiClient.Verify(c => c.CreatePathwayTypeEnrolmentAsync(It.Is<CreatePathwayTypeEnrolmentDto>(dto =>
      dto.ParticipantId == participantId &&
      dto.PathwayTypeId == pathwayParticipantDto.PathwayTypeId &&
      dto.PathwayTypeName == pathwayParticipantDto.PathwayTypeName &&
      dto.ScreeningName == pathwayParticipantDto.ScreeningName
    )), Times.Once);
    _mockEventGridPublisherClient.Verify(c => c.SendEventAsync(It.IsAny<EventGridEvent>(), default), Times.Once);
  }

  [Fact]
  public async Task Run_WithValidEvent_UsesExistingParticipant_WhenParticipantExists()
  {
    // Arrange
    var participantId = Guid.NewGuid();
    var pathwayParticipantDto = new CreatePathwayParticipantDto
    {
      NHSNumber = "1234567890",
      PathwayTypeId = Guid.NewGuid(),
      PathwayTypeName = "Test Pathway",
      ScreeningName = "Test Screening"
    };

    var existingParticipant = new ParticipantDto
    {
      ParticipantId = participantId
    };

    var eventGridEvent = new EventGridEvent(
      "test-subject",
      "test-event",
      "1.0",
      pathwayParticipantDto,
      typeof(CreatePathwayParticipantDto)
    );

    _mockCrudApiClient.Setup(c => c.GetParticipantByNhsNumberAsync(pathwayParticipantDto.NHSNumber))
      .ReturnsAsync(existingParticipant);

    _mockCrudApiClient.Setup(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()))
      .ReturnsAsync(true);

    _mockResponse.Setup(r => r.Status).Returns((int)HttpStatusCode.OK);

    _mockEventGridPublisherClient.Setup(c => c.SendEventAsync(It.IsAny<EventGridEvent>(), default))
      .ReturnsAsync(_mockResponse.Object);

    // Act
    await _handler.Run(eventGridEvent);

    // Assert
    _mockCrudApiClient.Verify(c => c.GetParticipantByNhsNumberAsync(pathwayParticipantDto.NHSNumber), Times.Once);
    _mockCrudApiClient.Verify(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()), Times.Never);
    _mockCrudApiClient.Verify(c => c.CreatePathwayTypeEnrolmentAsync(It.Is<CreatePathwayTypeEnrolmentDto>(dto =>
      dto.ParticipantId == participantId &&
      dto.PathwayTypeId == pathwayParticipantDto.PathwayTypeId &&
      dto.PathwayTypeName == pathwayParticipantDto.PathwayTypeName &&
      dto.ScreeningName == pathwayParticipantDto.ScreeningName
    )), Times.Once);
    _mockEventGridPublisherClient.Verify(c => c.SendEventAsync(It.IsAny<EventGridEvent>(), default), Times.Once);
  }

  [Fact]
  public async Task Run_WithInvalidEventData_LogsErrorAndReturns()
  {
    // Arrange
    var eventGridEvent = new EventGridEvent(
      "test-subject",
      "test-event",
      "1.0",
      "invalid json data"
    );

    // Act
    await _handler.Run(eventGridEvent);

    // Assert
    _mockCrudApiClient.Verify(c => c.GetParticipantByNhsNumberAsync(It.IsAny<string>()), Times.Never);
    _mockCrudApiClient.Verify(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()), Times.Never);
    _mockCrudApiClient.Verify(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()),
      Times.Never);
    _mockEventGridPublisherClient.Verify(c => c.SendEventAsync(It.IsAny<EventGridEvent>(), default), Times.Never);
  }

  [Fact]
  public async Task Run_WithNullPathwayParticipantDto_ReturnsEarly()
  {
    // Arrange
    var eventGridEvent = new EventGridEvent(
      "test-subject",
      "test-event",
      "1.0",
      "null"
    );

    // Act
    await _handler.Run(eventGridEvent);

    // Assert
    _mockCrudApiClient.Verify(c => c.GetParticipantByNhsNumberAsync(It.IsAny<string>()), Times.Never);
    _mockCrudApiClient.Verify(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()), Times.Never);
    _mockCrudApiClient.Verify(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()),
      Times.Never);
    _mockEventGridPublisherClient.Verify(c => c.SendEventAsync(It.IsAny<EventGridEvent>(), default), Times.Never);
  }

  [Fact]
  public async Task Run_WhenPathwayEnrolmentCreationFails_LogsErrorAndDoesNotSendEvent()
  {
    // Arrange
    var participantId = Guid.NewGuid();
    var pathwayParticipantDto = new CreatePathwayParticipantDto
    {
      NHSNumber = "1234567890",
      PathwayTypeId = Guid.NewGuid(),
      PathwayTypeName = "Test Pathway",
      ScreeningName = "Test Screening"
    };

    var eventGridEvent = new EventGridEvent(
      "test-subject",
      "test-event",
      "1.0",
      pathwayParticipantDto,
      typeof(CreatePathwayParticipantDto)
    );

    _mockCrudApiClient.Setup(c => c.GetParticipantByNhsNumberAsync(pathwayParticipantDto.NHSNumber))
      .ReturnsAsync((ParticipantDto)null);

    _mockCrudApiClient.Setup(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()))
      .ReturnsAsync(participantId);

    _mockCrudApiClient.Setup(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()))
      .ReturnsAsync(false);

    // Act
    await _handler.Run(eventGridEvent);

    // Assert
    _mockCrudApiClient.Verify(c => c.GetParticipantByNhsNumberAsync(pathwayParticipantDto.NHSNumber), Times.Once);
    _mockCrudApiClient.Verify(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()), Times.Once);
    _mockCrudApiClient.Verify(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()),
      Times.Once);
    _mockEventGridPublisherClient.Verify(c => c.SendEventAsync(It.IsAny<EventGridEvent>(), default), Times.Never);
  }

  [Fact]
  public async Task Run_WhenEventSendingFails_LogsError()
  {
    // Arrange
    var participantId = Guid.NewGuid();
    var pathwayParticipantDto = new CreatePathwayParticipantDto
    {
      NHSNumber = "1234567890",
      PathwayTypeId = Guid.NewGuid(),
      PathwayTypeName = "Test Pathway",
      ScreeningName = "Test Screening"
    };

    var eventGridEvent = new EventGridEvent(
      "test-subject",
      "test-event",
      "1.0",
      pathwayParticipantDto,
      typeof(CreatePathwayParticipantDto)
    );

    _mockCrudApiClient.Setup(c => c.GetParticipantByNhsNumberAsync(pathwayParticipantDto.NHSNumber))
      .ReturnsAsync((ParticipantDto)null);

    _mockCrudApiClient.Setup(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()))
      .ReturnsAsync(participantId);

    _mockCrudApiClient.Setup(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()))
      .ReturnsAsync(true);

    _mockResponse.Setup(r => r.Status).Returns((int)HttpStatusCode.BadRequest);

    _mockEventGridPublisherClient.Setup(c => c.SendEventAsync(It.IsAny<EventGridEvent>(), default))
      .ReturnsAsync(_mockResponse.Object);

    // Act
    await _handler.Run(eventGridEvent);

    // Assert
    _mockCrudApiClient.Verify(c => c.GetParticipantByNhsNumberAsync(pathwayParticipantDto.NHSNumber), Times.Once);
    _mockCrudApiClient.Verify(c => c.CreateParticipantAsync(It.IsAny<ParticipantDto>()), Times.Once);
    _mockCrudApiClient.Verify(c => c.CreatePathwayTypeEnrolmentAsync(It.IsAny<CreatePathwayTypeEnrolmentDto>()),
      Times.Once);
    _mockEventGridPublisherClient.Verify(c => c.SendEventAsync(It.IsAny<EventGridEvent>(), default), Times.Once);
  }
}
