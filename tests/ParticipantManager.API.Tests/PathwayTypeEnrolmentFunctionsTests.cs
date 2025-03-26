using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ParticipantManager.API.Data;
using ParticipantManager.API.Functions;
using ParticipantManager.API.Models;
using ParticipantManager.TestUtils;

namespace ParticipantManager.API.Tests;

public class PathwayTypeEnrolmentFunctionsTests
{
    private readonly ParticipantManagerDbContext _dbContext;
    private readonly Mock<ILogger<PathwayTypeEnrolmentFunctions>> _logger;
    private readonly PathwayTypeEnrolmentFunctions _function;
    private readonly SetupRequest _requestSetup = new();
    private readonly Participant _participant1;
    private readonly Participant _participant2;

    public PathwayTypeEnrolmentFunctionsTests()
    {
        // Create a new unique in-memory database for each test
        var databaseName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ParticipantManagerDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        _dbContext = new ParticipantManagerDbContext(options);
        _logger = new Mock<ILogger<PathwayTypeEnrolmentFunctions>>();
        _function = new PathwayTypeEnrolmentFunctions(_logger.Object, _dbContext, jsonOptions);

        _participant1 = new Participant
        {
            ParticipantId = Guid.NewGuid(),
            Name = "Participant One",
            DOB = new DateTime(1970, 1, 1),
            NhsNumber = "9990000001"
        };
        _dbContext.Participants.Add(_participant1);

        _participant2 = new Participant
        {
            ParticipantId = Guid.NewGuid(),
            Name = "Participant Two",
            DOB = new DateTime(1970, 1, 1),
            NhsNumber = "9990000002"
        };
        _dbContext.Participants.Add(_participant2);

        _dbContext.SaveChanges();
    }

    [Theory]
    [InlineData("")]
    [InlineData("participantId=")]
    public async Task GetPathwayTypeEnrolmentsByParticipantId_ReturnsBadRequest_WhenParticipantIdMissing(string queryString)
    {
        // Arrange
        var request = _requestSetup.CreateMockHttpRequestWithQuery(queryString);

        // Act
        var response = await _function.GetPathwayTypeEnrolmentsByParticipantId(request);

        // Assert
        var result = Assert.IsType<BadRequestObjectResult>(response);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task GetPathwayTypeEnrolmentsByParticipantId_ReturnsOkResultAndEmptyList_WhenParticipantHasNoEnrolments()
    {
        // Arrange
        var participantId = Guid.NewGuid();
        var request = _requestSetup.CreateMockHttpRequestWithQuery($"participantId={participantId}");

        // Act
        var response = await _function.GetPathwayTypeEnrolmentsByParticipantId(request);

        // Assert
        var result = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        var enrolments = result.Value as List<PathwayTypeEnrolment>;
        Assert.NotNull(enrolments);
        Assert.Empty(enrolments);
    }

    [Fact]
    public async Task GetPathwayTypeEnrolmentsByParticipantId_ReturnsOkResultAndAllEnrolmentsForParticipant_WhenParticipantHasEnrolments()
    {
        // Arrange
        CreateEnrolmentsForParticipant(_participant1, 5);
        CreateEnrolmentsForParticipant(_participant2, 3);

        var request = _requestSetup.CreateMockHttpRequestWithQuery($"participantId={_participant1.ParticipantId}");

        // Act
        var response = await _function.GetPathwayTypeEnrolmentsByParticipantId(request);

        // Assert
        var result = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        var enrolments = result.Value as List<PathwayTypeEnrolment>;
        Assert.NotNull(enrolments);
        Assert.Equal(5, enrolments.Count);
        Assert.True(enrolments.All(e => e.ParticipantId == _participant1.ParticipantId));
    }

    [Fact]
    public async Task GetPathwayTypeEnrolmentById_ReturnsNotFound_WhenNoEnrolmentWithId()
    {
        // Arrange
        CreateEnrolmentsForParticipant(_participant1, 3);
        var enrolmentId = Guid.NewGuid();
        var request = _requestSetup.CreateMockHttpRequest(null);

        // Act
        var response = await _function.GetPathwayTypeEnrolmentById(request, _participant1.ParticipantId, enrolmentId);

        // Assert
        var result = Assert.IsType<NotFoundObjectResult>(response);
        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
    }

    [Fact]
    public async Task GetPathwayTypeEnrolmentById_ReturnsOk_WhenEnrolmentFound()
    {
        // Arrange
        CreateEnrolmentsForParticipant(_participant1, 3);
        var enrolmentId = _dbContext.PathwayTypeEnrolments.First().EnrolmentId;
        var request = _requestSetup.CreateMockHttpRequest(null);

        // Act
        var response = await _function.GetPathwayTypeEnrolmentById(request, _participant1.ParticipantId, enrolmentId);

        // Assert
        var result = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        var enrolment = result.Value as PathwayTypeEnrolment;
        Assert.Equal(enrolmentId, enrolment?.EnrolmentId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task CreatePathwayTypeEnrolment_ReturnsBadRequest_WhenPathwayTypeEnrolmentEmpty(string? requestBody)
    {
        // Arrange
        var request = _requestSetup.CreateMockHttpRequest(requestBody);

        // Act
        var response = await _function.CreatePathwayTypeEnrolment(request);

        // Assert
        var result = Assert.IsType<BadRequestObjectResult>(response);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Empty(_dbContext.PathwayTypeEnrolments);
    }

    [Fact]
    public async Task CreatePathwayTypeEnrolment_ReturnsBadRequest_WhenPathwayTypeEnrolmentIsNull()
    {
        // Arrange
        var request = _requestSetup.CreateMockHttpRequest(null);

        // Act
        var response = await _function.CreatePathwayTypeEnrolment(request);

        // Assert
        var result = Assert.IsType<BadRequestObjectResult>(response);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task CreatePathwayTypeEnrolment_ReturnsBadRequest_WhenPathwayTypeEnrolmentInvalid()
    {
        // Arrange
        var invalidEnrolment = new
        {
            ParticipantId = _participant1.ParticipantId,
            PathwayTypeId = Guid.NewGuid(),
            ScreeningName = "Testing",
        };
        var request = _requestSetup.CreateMockHttpRequest(invalidEnrolment);

        // Act
        var response = await _function.CreatePathwayTypeEnrolment(request);

        // Assert
        var result = Assert.IsType<BadRequestObjectResult>(response);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task CreatePathwayTypeEnrolment_CreatesEnrolment_WhenPathwayTypeEnrolmentValid()
    {
        // Arrange
        var enrolment = new PathwayTypeEnrolment
        {
            ParticipantId = _participant1.ParticipantId,
            PathwayTypeId = Guid.NewGuid(),
            ScreeningName = "Testing",
            PathwayTypeName = "Testing"
        };
        var request = _requestSetup.CreateMockHttpRequest(enrolment);

        // Act
        var response = await _function.CreatePathwayTypeEnrolment(request);

        // Assert
        var result = Assert.IsType<CreatedResult>(response);
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.Equal(1, _dbContext.PathwayTypeEnrolments.Where(e => e.ParticipantId == _participant1.ParticipantId).Count());
    }

    private void CreateEnrolmentsForParticipant(Participant participant, int numberOfEnrolments)
    {
        for (var i = 0; i < numberOfEnrolments; i++)
        {
            _dbContext.PathwayTypeEnrolments.Add(new PathwayTypeEnrolment
            {
                ParticipantId = participant.ParticipantId,
                PathwayTypeId = Guid.NewGuid(),
                Participant = participant,
                ScreeningName = "Testing",
                PathwayTypeName = "Testing"
            });
        }

        _dbContext.SaveChanges();
    }
}
