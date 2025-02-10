using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ParticipantManager.API.Data;
using ParticipantManager.API.Functions;
using ParticipantManager.API.Models;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ParticipantManager.API.Tests
{
    public class ParticipantFunctionsTests
    {

      private ServiceProvider CreateServiceProvider(string databaseName)
      {
        var services = new ServiceCollection();

        // Use a unique in-memory database per test to isolate them
        services.AddDbContext<ParticipantManagerDbContext>(options =>
          options.UseInMemoryDatabase(databaseName));

        services.AddLogging(builder =>
        {
          builder.AddConsole(); // Use console logging
          builder.SetMinimumLevel(LogLevel.Debug);
        });

        services.AddScoped<ParticipantFunctions>(); // Register the function

        return services.BuildServiceProvider();
      }

      private HttpRequestData CreateMockHttpRequest(FunctionContext functionContext, object body)
      {
        var json = JsonSerializer.Serialize(body);
        var byteArray = Encoding.UTF8.GetBytes(json);
        var memoryStream = new MemoryStream(byteArray);

        var mockRequest = new Mock<HttpRequestData>(MockBehavior.Strict, functionContext);
        mockRequest.Setup(r => r.Body).Returns(memoryStream);
        return mockRequest.Object;
      }

      private HttpRequestData CreateMockHttpRequestWithQuery(FunctionContext functionContext, string queryString)
      {
        var requestUrl = new Uri($"http://localhost/api/participants?{queryString}");
        var mockRequest = new Mock<HttpRequestData>(MockBehavior.Strict, functionContext);
        mockRequest.Setup(r => r.Url).Returns(requestUrl);
        mockRequest.Setup(r => r.Headers).Returns(new HttpHeadersCollection());
        return mockRequest.Object;
      }

        [Fact]
        public async Task CreateParticipant_ShouldAddParticipantToDatabase()
        {
            var serviceProvider = CreateServiceProvider(Guid.NewGuid().ToString());
            var dbContext = serviceProvider.GetRequiredService<ParticipantManagerDbContext>();
            var logger = new Mock<ILogger<ParticipantFunctions>>();
            var functionContext = new Mock<FunctionContext>().Object;

            var fun = new ParticipantFunctions(logger.Object, dbContext);

            // Mock HTTP Request
            var participant = new Participant()
            {
                Name = "Jane Doe",
                DOB = DateTime.Parse("1985-05-15"),
                NHSNumber = "1395608539"
            };

            var request = CreateMockHttpRequest(functionContext, participant);

            // Act
            var response = await fun.CreateParticipant(request) as CreatedResult;

            Assert.Equal(StatusCodes.Status201Created, response?.StatusCode);
            Assert.Contains(dbContext.Participants, p => p.NHSNumber == "1395608539");
        }

        [Fact]
        public async Task CreateParticipant_ShouldReturnConflict_WhenNHSNumberAlreadyExists()
        {
          // Arrange
          var serviceProvider = CreateServiceProvider(Guid.NewGuid().ToString());
          var dbContext = serviceProvider.GetRequiredService<ParticipantManagerDbContext>();
          var logger = new Mock<ILogger<ParticipantFunctions>>();
          var functionContext = new Mock<FunctionContext>().Object;

          var fun = new ParticipantFunctions(logger.Object, dbContext);

          // Mock HTTP Request
          var participant = new Participant()
          {
            Name = "Jane Doe",
            DOB = DateTime.Parse("1985-05-15"),
            NHSNumber = "1395608539"
          };

          // Create data in database directly
          dbContext.Participants.Add(participant);
          await dbContext.SaveChangesAsync();

          var request = CreateMockHttpRequest(functionContext, participant);
          // Act
          var response = await fun.CreateParticipant(request) as ConflictObjectResult;
          Assert.Equal(StatusCodes.Status409Conflict, response?.StatusCode);
          Assert.Contains("A participant with this NHS Number already exists.", response?.Value.ToString());

        }

        [Fact]
        public async Task GetParticipantById_ShouldReturnParticipant_WhenValidIdProvided()
        {
          // Arrange
          var serviceProvider = CreateServiceProvider(Guid.NewGuid().ToString());
          var dbContext = serviceProvider.GetRequiredService<ParticipantManagerDbContext>();
          var logger = new Mock<ILogger<ParticipantFunctions>>();
          var functionContext = new Mock<FunctionContext>().Object;

          var function = new ParticipantFunctions(logger.Object, dbContext);

          var participant = new Participant
          {
            Name = "John Doe",
            DOB = DateTime.Parse("1980-01-01"),
            NHSNumber = "1234567890"
          };

          dbContext.Participants.Add(participant);
          await dbContext.SaveChangesAsync();

          var request = CreateMockHttpRequest(functionContext, null);

          // Act
          var response = await function.GetParticipantById(request, participant.ParticipantId);

          // Assert
          var result = Assert.IsType<OkObjectResult>(response);
          var returnedParticipant = Assert.IsType<Participant>(result.Value);
          Assert.Equal(participant.ParticipantId, returnedParticipant.ParticipantId);
        }

        [Fact]
        public async Task GetParticipantByNhsNumber_ShouldReturnParticipant_WhenValidNhsNumberProvided()
        {
          // Arrange
          var serviceProvider = CreateServiceProvider(Guid.NewGuid().ToString());
          var dbContext = serviceProvider.GetRequiredService<ParticipantManagerDbContext>();
          var logger = new Mock<ILogger<ParticipantFunctions>>();
          var functionContext = new Mock<FunctionContext>().Object;

          var function = new ParticipantFunctions(logger.Object, dbContext);

          var participant = new Participant
          {
            Name = "Jane Doe",
            DOB = DateTime.Parse("1990-06-15"),
            NHSNumber = "9876543210"
          };

          dbContext.Participants.Add(participant);
          await dbContext.SaveChangesAsync();

          var request = CreateMockHttpRequestWithQuery(functionContext, "nhsNumber=9876543210");

          // Act
          var response = await function.GetParticipantByNhsNumber(request);

          // Assert
          var result = Assert.IsType<OkObjectResult>(response);
          var returnedParticipant = Assert.IsType<Participant>(result.Value);
          Assert.Equal(participant.NHSNumber, returnedParticipant.NHSNumber);
        }

        [Fact]
        public async Task GetParticipantById_ShouldReturnNotFound_WhenParticipantDoesNotExist()
        {
          // Arrange
          var serviceProvider = CreateServiceProvider(Guid.NewGuid().ToString());
          var dbContext = serviceProvider.GetRequiredService<ParticipantManagerDbContext>();
          var logger = new Mock<ILogger<ParticipantFunctions>>();
          var functionContext = new Mock<FunctionContext>().Object;

          var function = new ParticipantFunctions(logger.Object, dbContext);
          var request = CreateMockHttpRequest(functionContext, null);

          // Act
          var response = await function.GetParticipantById(request, Guid.NewGuid());

          // Assert
          var result = Assert.IsType<NotFoundObjectResult>(response);
          Assert.Contains("not found", result.Value.ToString());
        }

        [Fact]
        public async Task GetParticipantByNhsNumber_ShouldReturnBadRequest_WhenNhsNumberIsMissing()
        {
          // Arrange
          var serviceProvider = CreateServiceProvider(Guid.NewGuid().ToString());
          var dbContext = serviceProvider.GetRequiredService<ParticipantManagerDbContext>();
          var logger = new Mock<ILogger<ParticipantFunctions>>();
          var functionContext = new Mock<FunctionContext>().Object;

          var function = new ParticipantFunctions(logger.Object, dbContext);
          var request = CreateMockHttpRequestWithQuery(functionContext, ""); // No query param

          // Act
          var response = await function.GetParticipantByNhsNumber(request);

          // Assert
          var result = Assert.IsType<BadRequestObjectResult>(response);
          Assert.Contains("Please provide an NHS Number", result.Value.ToString());
        }
    }
}
