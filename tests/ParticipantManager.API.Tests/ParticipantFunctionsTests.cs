namespace ParticipantManager.API.Tests;

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ParticipantManager.API.Data;
using ParticipantManager.API.Functions;
using ParticipantManager.API.Models;

public class ParticipantFunctionsTests
{
  private readonly ParticipantManagerDbContext _dbContext;
  private readonly Mock<ILogger<ParticipantFunctions>> _logger;
  private readonly FunctionContext _functionContext;
  private readonly ParticipantFunctions _function;
  private readonly Participant _mockParticipant;
  private readonly Participant _mockParticipant2;

  public ParticipantFunctionsTests()
  {
    _mockParticipant = new Participant
    {
      ParticipantId = Guid.NewGuid(),
      Name = "Test Ting",
      DOB = DateTime.Parse("1980-01-01"),
      NhsNumber = "1234567890"
    };

    _mockParticipant2 = new Participant
    {
      ParticipantId = Guid.NewGuid(),
      Name = "Justin Test",
      DOB = DateTime.Parse("1990-06-15"),
      NhsNumber = "9876543210"
    };

    // Create a new unique in-memory database for each test
    var databaseName = Guid.NewGuid().ToString();
    var options = new DbContextOptionsBuilder<ParticipantManagerDbContext>()
        .UseInMemoryDatabase(databaseName)
        .Options;

    var jsonOptions = new JsonSerializerOptions {
      PropertyNameCaseInsensitive = true
    };

    _dbContext = new ParticipantManagerDbContext(options);
    _logger = new Mock<ILogger<ParticipantFunctions>>();
    _functionContext = new Mock<FunctionContext>().Object;
    _function = new ParticipantFunctions(_logger.Object, _dbContext, jsonOptions);
  }

  private static HttpRequestData CreateMockHttpRequest(FunctionContext functionContext, object? body)
  {
    var json = JsonSerializer.Serialize(body);
    var byteArray = Encoding.UTF8.GetBytes(json);
    var memoryStream = new MemoryStream(byteArray);
    var mockRequest = new Mock<HttpRequestData>(MockBehavior.Strict, functionContext);
    mockRequest.Setup(r => r.Body).Returns(memoryStream);
    return mockRequest.Object;
  }

  private static HttpRequestData CreateMockHttpRequestWithQuery(FunctionContext functionContext, string queryString)
  {
    var requestUrl = new Uri($"http://localhost/api/participants?{queryString}");
    var mockRequest = new Mock<HttpRequestData>(MockBehavior.Strict, functionContext);
    mockRequest.Setup(r => r.Url).Returns(requestUrl);
    mockRequest.Setup(r => r.Headers).Returns(new HttpHeadersCollection());
    return mockRequest.Object;
  }

  [Fact]
  public async Task CreateParticipant_ValidParticipantData_ReturnsCreatedResult()
  {
    // Arrange
    var request = CreateMockHttpRequest(_functionContext, _mockParticipant);

    // Act
    var response = await _function.CreateParticipant(request);

    // Assert
    var result = Assert.IsType<CreatedResult>(response);
    Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
    Assert.Contains(_dbContext.Participants, p => p.NhsNumber == _mockParticipant.NhsNumber);
  }

  [Fact]
  public async Task GetParticipantById_ValidParticipantId_ReturnsOkWithParticipant()
  {
    // Arrange
    _dbContext.Participants.Add(_mockParticipant);
    await _dbContext.SaveChangesAsync();

    var request = CreateMockHttpRequest(_functionContext, null);

    // Act
    var response = await _function.GetParticipantById(request, _mockParticipant.ParticipantId);

    // Assert
    var result = Assert.IsType<OkObjectResult>(response);
    var returnedParticipant = Assert.IsType<Participant>(result.Value);
    Assert.Equal(_mockParticipant.ParticipantId, returnedParticipant.ParticipantId);
  }

  [Fact]
  public async Task GetParticipantByNhsNumber_ValidNhsNumber_ReturnsOkWithParticipant()
  {
    // Arrange
    _dbContext.Participants.Add(_mockParticipant2);
    await _dbContext.SaveChangesAsync();

    var request = CreateMockHttpRequestWithQuery(_functionContext, $"nhsNumber={_mockParticipant2.NhsNumber}");

    // Act
    var response = await _function.GetParticipantByNhsNumber(request);

    // Assert
    var result = Assert.IsType<OkObjectResult>(response);
    var returnedParticipant = Assert.IsType<Participant>(result.Value);
    Assert.Equal(_mockParticipant2.NhsNumber, returnedParticipant.NhsNumber);
  }

  [Theory]
  [InlineData("nhsNumber=9999999999", typeof(NotFoundObjectResult), "No Participant found with NHS Number")]
  [InlineData("", typeof(BadRequestObjectResult), "Please provide an NHS Number")]
  public async Task GetParticipantByNhsNumber_InvalidParameters_ReturnsExpectedErrorResult(
      string queryParam, Type expectedResultType, string expectedMessage)
  {
    // Arrange
    var request = CreateMockHttpRequestWithQuery(_functionContext, queryParam);

    // Act
    var response = await _function.GetParticipantByNhsNumber(request);

    // Assert
    Assert.IsType(expectedResultType, response);
    var result = (ObjectResult)response;
    Assert.Contains(expectedMessage, result.Value?.ToString());
  }

  [Theory]
  [InlineData("{\"Name\": \"Test User\", \"DOB\": \"1990-01-01\", \"NhsNumber\": \"1234567890", typeof(BadRequestObjectResult), "An error occurred")]
  public async Task CreateParticipant_InvalidRequestBody_ReturnsBadRequest(
      string requestBody, Type expectedResultType, string expectedMessage)
  {
    // Arrange
    var mockRequest = CreateMockHttpRequest(_functionContext, requestBody);

    // Act
    var response = await _function.CreateParticipant(mockRequest);

    // Assert
    Assert.IsType(expectedResultType, response);
    var result = (ObjectResult)response;
    Assert.Contains(expectedMessage, result.Value?.ToString());
  }

  [Theory]
  [InlineData("", "", "1234567890", typeof(List<ValidationResult>))]
  [InlineData("John Doe", "", "", typeof(List<ValidationResult>))]
  [InlineData("", "1980-01-01", "", typeof(List<ValidationResult>))]
  public async Task CreateParticipant_MissingRequiredFields_ReturnsBadRequestWithValidationErrors(
      string name, string dobString, string nhsNumber, Type expectedValidationResultType)
  {
    // Arrange
    var invalidParticipant = new Participant() {Name = "", NhsNumber = ""};

    if (!string.IsNullOrEmpty(name))
      invalidParticipant.Name = name;

    if (!string.IsNullOrEmpty(dobString))
      invalidParticipant.DOB = DateTime.Parse(dobString);

    if (!string.IsNullOrEmpty(nhsNumber))
      invalidParticipant.NhsNumber = nhsNumber;

    var request = CreateMockHttpRequest(_functionContext, invalidParticipant);

    // Act
    var response = await _function.CreateParticipant(request);

    // Assert
    var result = Assert.IsType<BadRequestObjectResult>(response);
    Assert.IsType(expectedValidationResultType, result.Value);
  }

  [Fact]
  public async Task CreateParticipant_DuplicateNhsNumber_ReturnsConflict()
  {
    // Arrange
    _dbContext.Participants.Add(_mockParticipant);
    await _dbContext.SaveChangesAsync();

    var request = CreateMockHttpRequest(_functionContext, _mockParticipant);

    // Act
    var response = await _function.CreateParticipant(request);

    // Assert
    var result = Assert.IsType<ConflictObjectResult>(response);
    Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
    Assert.Contains("A Participant with this NHS Number already exists.", result.Value?.ToString());
  }

  [Fact]
  public async Task GetParticipantById_NonMatchingParticipantId_ReturnsNotFound()
  {
    // Arrange
    var participantId = Guid.NewGuid();
    var request = CreateMockHttpRequest(_functionContext, null);

    // Act
    var response = await _function.GetParticipantById(request, participantId);

    // Assert
    var result = Assert.IsType<NotFoundObjectResult>(response);
    Assert.NotNull(result);
    Assert.Contains("not found", result.Value?.ToString());
  }
}
