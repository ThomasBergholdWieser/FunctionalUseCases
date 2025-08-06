using Microsoft.Extensions.Logging;

namespace FunctionalUseCases.Tests;

public class ExecutionErrorTests
{
    [Fact]
    public void ExecutionError_Constructor_WithSingleMessage_ShouldSetMessage()
    {
        // Arrange
        const string message = "Test error";

        // Act
        var error = new ExecutionError(message);

        // Assert
        Assert.Equal(message, error.Message);
        Assert.Single(error.Messages);
        Assert.Equal(message, error.Messages[0]);
    }

    [Fact]
    public void ExecutionError_Constructor_WithMultipleMessages_ShouldJoinMessages()
    {
        // Arrange
        var messages = new[] { "Error 1", "Error 2", "Error 3" };

        // Act
        var error = new ExecutionError(messages);

        // Assert
        Assert.Equal("Error 1; Error 2; Error 3", error.Message);
        Assert.Equal(3, error.Messages.Count);
        Assert.Equal(messages, error.Messages);
    }

    [Fact]
    public void ExecutionError_Constructor_WithEnumerable_ShouldSetMessages()
    {
        // Arrange
        var messages = new List<string> { "Error 1", "Error 2" };

        // Act
        var error = new ExecutionError(messages);

        // Assert
        Assert.Equal(2, error.Messages.Count);
        Assert.Equal("Error 1; Error 2", error.Message);
    }

    [Fact]
    public void ExecutionError_Properties_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var error = new ExecutionError("test");

        // Assert
        Assert.Null(error.ErrorCode);
        Assert.Equal(LogLevel.Error, error.LogLevel);
        Assert.False(error.Logged);
    }

    [Fact]
    public void ExecutionError_Properties_ShouldBeSettable()
    {
        // Arrange
        var error = new ExecutionError("test");

        // Act
        error.ErrorCode = 404;
        error.LogLevel = LogLevel.Warning;

        // Assert
        Assert.Equal(404, error.ErrorCode);
        Assert.Equal(LogLevel.Warning, error.LogLevel);
    }
}

public class ExecutionErrorGenericTests
{
    [Fact]
    public void ExecutionError_Generic_WithCustomType_ShouldWork()
    {
        // Arrange
        var customMessages = new[] { 1, 2, 3 };

        // Act
        var error = new ExecutionError<int>(customMessages);

        // Assert
        Assert.Equal("1; 2; 3", error.Message);
        Assert.Equal(3, error.Messages.Count);
    }

    [Fact]
    public void ExecutionError_Generic_WithEnumerable_ShouldWork()
    {
        // Arrange
        var customMessages = new List<int> { 100, 200 };

        // Act
        var error = new ExecutionError<int>(customMessages);

        // Assert
        Assert.Equal("100; 200", error.Message);
        Assert.Equal(2, error.Messages.Count);
    }
}