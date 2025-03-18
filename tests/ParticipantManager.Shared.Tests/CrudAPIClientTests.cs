using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using ParticipantManager.Shared.Client;
using ParticipantManager.Shared.DTOs;

namespace ParticipantManager.Shared;

public class CrudApiClientTests
{
  private readonly Mock<ILogger<CrudApiClient>> _mockLogger;
  private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
  private readonly HttpClient _httpClient;
  private readonly CrudApiClient _client;

  public CrudApiClientTests()
  {
    _mockLogger = new Mock<ILogger<CrudApiClient>>();
    _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
    {
      BaseAddress = new Uri("https://testapi.com")
    };

    _client = new CrudApiClient(_mockLogger.Object, _httpClient);
  }

  [Fact]
  public async Task GetPathwayEnrolmentsAsync_ShouldReturnEnrolments_WhenResponseIsSuccessful()
  {
    // Arrange
    var participantId = Guid.NewGuid();
    var expectedEnrolments = new List<PathwayEnrolmentDto>
    {
      new PathwayEnrolmentDto { EnrolmentId = Guid.NewGuid(), ScreeningName = "Test Pathway" }
    };

    _mockHttpMessageHandler.SetupRequest(HttpMethod.Get, $"/api/pathwaytypeenrolments?participantId={participantId}")
      .ReturnsResponse(HttpStatusCode.OK, expectedEnrolments);

    // Act
    var result = await _client.GetPathwayEnrolmentsAsync(participantId);

    // Assert
    Assert.NotNull(result);
    Assert.Single(result);
    Assert.Equal("Test Pathway", result[0].PathwayTypeName);
  }

  [Fact]
  public async Task GetPathwayEnrolmentByIdAsync_ShouldReturnEnrolment_WhenResponseIsSuccessful()
  {
    // Arrange
    var participantId = Guid.NewGuid();
    var enrolmentId = Guid.NewGuid().ToString();
    var expectedEnrolment = new EnrolledPathwayDetailsDto { EnrolmentId = enrolmentId };

    _mockHttpMessageHandler.SetupRequest(HttpMethod.Get, $"/api/participants/{participantId}/pathwaytypeenrolments/{enrolmentId}")
      .ReturnsResponse(HttpStatusCode.OK, expectedEnrolment);

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
    var expectedParticipant = new ParticipantDto { ParticipantId = Guid.NewGuid(), NHSNumber = nhsNumber };

    _mockHttpMessageHandler.SetupRequest(HttpMethod.Get, $"/api/participants?nhsNumber={nhsNumber}")
      .ReturnsResponse(HttpStatusCode.OK, expectedParticipant);

    // Act
    var result = await _client.GetParticipantByNhsNumberAsync(nhsNumber);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(nhsNumber, result.NHSNumber);
  }

  [Fact]
  public async Task CreateParticipantAsync_ShouldReturnParticipantId_WhenSuccessful()
  {
    // Arrange
    var participantDto = new ParticipantDto { NHSNumber = "1234567890" };
    var responseDto = new ParticipantDto { ParticipantId = Guid.NewGuid() };

    _mockHttpMessageHandler.SetupRequest(HttpMethod.Post, "/api/participants", participantDto)
      .ReturnsResponse(HttpStatusCode.Created, responseDto);

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
      PathwayTypeName = "Test Pathway"
    };

    _mockHttpMessageHandler.SetupRequest(HttpMethod.Post, "/api/pathwaytypeenrolment", enrolmentDto)
      .ReturnsResponse(HttpStatusCode.OK);

    // Act
    var result = await _client.CreatePathwayTypeEnrolmentAsync(enrolmentDto);

    // Assert
    Assert.True(result);
  }
}
