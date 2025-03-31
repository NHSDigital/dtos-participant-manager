using Microsoft.Azure.Functions.Worker;
using Moq;

namespace ParticipantManager.Shared;

public class FunctionContextAccessorTests
{
    [Fact]
    public void FunctionContext_WhenSet_BeRetrievable()
    {
        // Arrange
        var accessor = new FunctionContextAccessor();
        var mockContext = new Mock<FunctionContext>().Object;

        // Act
        accessor.FunctionContext = mockContext;
        var retrievedContext = accessor.FunctionContext;

        // Assert
        Assert.Same(mockContext, retrievedContext);
    }

    [Fact]
    public void FunctionContext_WhenSetToNull_ReturnsNull()
    {
        // Arrange
        var accessor = new FunctionContextAccessor();
        var mockContext = new Mock<FunctionContext>().Object;

        // Set initial value
        accessor.FunctionContext = mockContext;

        // Act
        accessor.FunctionContext = null;

        // Assert
        Assert.Null(accessor.FunctionContext);
    }

    [Fact]
    public void FunctionContext_WhenSetMultipleTimes_ReturnsMostRecentValue()
    {
        // Arrange
        var accessor = new FunctionContextAccessor();
        var firstContext = new Mock<FunctionContext>().Object;
        var secondContext = new Mock<FunctionContext>().Object;

        // Act
        accessor.FunctionContext = firstContext;
        accessor.FunctionContext = secondContext;

        // Assert
        Assert.Same(secondContext, accessor.FunctionContext);
    }

    [Fact]
    public async Task FunctionContext_AccessAcrossAsyncBoundaries_MaintainsReference()
    {
        // Arrange
        var accessor = new FunctionContextAccessor();
        var mockContext = new Mock<FunctionContext>().Object;

        // Act
        accessor.FunctionContext = mockContext;

        // Verify initial state
        Assert.Same(mockContext, accessor.FunctionContext);

        // Verify context flows across async boundaries
        var asyncResult = await Task.Run(() => accessor.FunctionContext);

        // Assert
        Assert.Same(mockContext, asyncResult);
    }

    [Fact]
    public void FunctionContext_WhenReplacingNonNullContext_ClearPreviousContext()
    {
        // Arrange
        var accessor = new FunctionContextAccessor();
        var firstContext = new Mock<FunctionContext>().Object;
        var secondContext = new Mock<FunctionContext>().Object;

        // Act - set first context
        accessor.FunctionContext = firstContext;

        // Verify initial state
        Assert.Same(firstContext, accessor.FunctionContext);

        // Act - replace with second context
        accessor.FunctionContext = secondContext;

        // Assert
        Assert.Same(secondContext, accessor.FunctionContext);
    }
}

