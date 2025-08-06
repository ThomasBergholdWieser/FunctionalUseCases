using Microsoft.Extensions.Logging;

namespace FunctionalUseCases.Tests;

public class ExecutionTests
{
    [Fact]
    public void Execution_Success_ShouldReturnSuccessfulResult()
    {
        // Act
        var result = Execution.Success();

        // Assert
        Assert.True(result.ExecutionSucceeded);
        Assert.False(result.ExecutionFailed);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Execution_Success_WithValue_ShouldReturnSuccessfulResultWithValue()
    {
        // Arrange
        const string value = "test value";

        // Act
        var result = Execution.Success(value);

        // Assert
        Assert.True(result.ExecutionSucceeded);
        Assert.False(result.ExecutionFailed);
        Assert.Null(result.Error);
        Assert.Equal(value, result.CheckedValue);
    }

    [Fact]
    public void Execution_Failure_WithSingleMessage_ShouldReturnFailedResult()
    {
        // Arrange
        const string message = "Test error";

        // Act
        var result = Execution.Failure(message);

        // Assert
        Assert.False(result.ExecutionSucceeded);
        Assert.True(result.ExecutionFailed);
        Assert.NotNull(result.Error);
        Assert.Equal(message, result.Error.Message);
    }

    [Fact]
    public void Execution_Failure_WithMultipleMessages_ShouldReturnFailedResult()
    {
        // Arrange
        var messages = new[] { "Error 1", "Error 2" };

        // Act
        var result = Execution.Failure(messages);

        // Assert
        Assert.False(result.ExecutionSucceeded);
        Assert.True(result.ExecutionFailed);
        Assert.NotNull(result.Error);
        Assert.Equal("Error 1; Error 2", result.Error.Message);
    }

    [Fact]
    public void Execution_Failure_WithErrorCodeAndLogLevel_ShouldSetProperties()
    {
        // Arrange
        const string message = "Test error";
        const int errorCode = 404;
        const LogLevel logLevel = LogLevel.Warning;

        // Act
        var result = Execution.Failure(message, errorCode, logLevel);

        // Assert
        Assert.NotNull(result.Error);
        Assert.Equal(errorCode, result.Error.ErrorCode);
        Assert.Equal(logLevel, result.Error.LogLevel);
    }

    [Fact]
    public void Execution_Failure_WithException_ShouldExtractMessage()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        var result = Execution.Failure<string>(exception);

        // Assert
        Assert.False(result.ExecutionSucceeded);
        Assert.True(result.ExecutionFailed);
        Assert.NotNull(result.Error);
        Assert.Contains("Test exception", result.Error.Message);
    }

    [Fact]
    public void Execution_Failure_WithMessageAndException_ShouldCombineMessages()
    {
        // Arrange
        const string message = "Custom error";
        var exception = new InvalidOperationException("Test exception");

        // Act
        var result = Execution.Failure<string>(message, exception);

        // Assert
        Assert.NotNull(result.Error);
        Assert.Contains("Custom error", result.Error.Message);
        Assert.Contains("Test exception", result.Error.Message);
    }

    [Fact]
    public void Execution_Failure_WithAggregateException_ShouldExtractAllMessages()
    {
        // Arrange
        var innerExceptions = new Exception[]
        {
            new InvalidOperationException("Error 1"),
            new ArgumentException("Error 2")
        };
        var aggregateException = new AggregateException(innerExceptions);

        // Act
        var result = Execution.Failure<string>(aggregateException);

        // Assert
        Assert.NotNull(result.Error);
        Assert.Contains("Error 1", result.Error.Message);
        Assert.Contains("Error 2", result.Error.Message);
    }

    [Fact]
    public void Execution_Combine_WithAllSuccessful_ShouldReturnSuccess()
    {
        // Arrange
        var result1 = Execution.Success();
        var result2 = Execution.Success();
        var result3 = Execution.Success();

        // Act
        var combined = Execution.Combine(result1, result2, result3);

        // Assert
        Assert.True(combined.ExecutionSucceeded);
        Assert.Null(combined.Error);
    }

    [Fact]
    public void Execution_Combine_WithAnyFailed_ShouldReturnFailure()
    {
        // Arrange
        var successResult = Execution.Success();
        var failureResult = Execution.Failure("Test error");

        // Act
        var combined = Execution.Combine(successResult, failureResult);

        // Assert
        Assert.False(combined.ExecutionSucceeded);
        Assert.True(combined.ExecutionFailed);
        Assert.NotNull(combined.Error);
        Assert.Contains("Test error", combined.Error.Message);
    }

    [Fact]
    public void Execution_Combine_WithMultipleFailures_ShouldCombineMessages()
    {
        // Arrange
        var failure1 = Execution.Failure("Error 1");
        var failure2 = Execution.Failure("Error 2");

        // Act
        var combined = Execution.Combine(failure1, failure2);

        // Assert
        Assert.False(combined.ExecutionSucceeded);
        Assert.NotNull(combined.Error);
        Assert.Contains("Error 1", combined.Error.Message);
        Assert.Contains("Error 2", combined.Error.Message);
    }
}