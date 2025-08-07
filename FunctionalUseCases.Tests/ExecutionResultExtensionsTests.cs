using Microsoft.Extensions.Logging;
using FunctionalUseCases.Extensions;

namespace FunctionalUseCases.Tests;

public class ExecutionResultExtensionsTests
{
    [Fact]
    public void NoLog_ShouldSetNoLogProperty()
    {
        // Arrange
        var result = Execution.Success();

        // Act
        var noLogResult = result.NoLog();

        // Assert
        Assert.True(noLogResult.NoLog);
        Assert.Same(result, noLogResult); // Should return the same instance
    }

    [Fact]
    public void AsTask_ShouldReturnCompletedTask()
    {
        // Arrange
        var result = Execution.Success("test value");

        // Act
        var task = result.AsTask();

        // Assert
        Assert.True(task.IsCompleted);
        Assert.Equal(result, task.Result);
    }

    [Fact]
    public void Log_WithSuccessfulResult_ShouldNotLog()
    {
        // Arrange
        var result = Execution.Success();
        var mockLogger = new TestLogger();

        // Act
        var loggedResult = result.Log(mockLogger);

        // Assert
        Assert.Same(result, loggedResult);
        Assert.Empty(mockLogger.LoggedMessages);
    }

    [Fact]
    public void Log_WithFailedResult_ShouldLogError()
    {
        // Arrange
        const string errorMessage = "Test error";
        var result = Execution.Failure(errorMessage);
        var mockLogger = new TestLogger();

        // Act
        var loggedResult = result.Log(mockLogger);

        // Assert
        Assert.Same(result, loggedResult);
        Assert.NotEmpty(mockLogger.LoggedMessages);
        Assert.Contains(mockLogger.LoggedMessages, m => m.Message.Contains(errorMessage) && m.LogLevel == LogLevel.Error);
        Assert.True(result.Error!.Logged);
    }

    [Fact]
    public void Log_WithAlreadyLoggedResult_ShouldNotLogAgain()
    {
        // Arrange
        const string errorMessage = "Test error";
        var result = Execution.Failure(errorMessage);
        var mockLogger = new TestLogger();

        // Log it first to set the Logged flag
        result.Log(mockLogger);
        mockLogger.LoggedMessages.Clear(); // Clear the log to test the second call

        // Act - log again
        var loggedResult = result.Log(mockLogger);

        // Assert
        Assert.Same(result, loggedResult);
        Assert.Empty(mockLogger.LoggedMessages); // Should not log again
    }

    [Fact]
    public void Log_WithDifferentLogLevels_ShouldLogAtCorrectLevel()
    {
        // Arrange
        var warningResult = Execution.Failure("Warning message", logLevel: LogLevel.Warning);
        var infoResult = Execution.Failure("Info message", logLevel: LogLevel.Information);
        var mockLogger = new TestLogger();

        // Act
        warningResult.Log(mockLogger);
        infoResult.Log(mockLogger);

        // Assert
        Assert.Equal(2, mockLogger.LoggedMessages.Count);
        Assert.Contains(mockLogger.LoggedMessages, m => m.LogLevel == LogLevel.Warning);
        Assert.Contains(mockLogger.LoggedMessages, m => m.LogLevel == LogLevel.Information);
    }

    private class TestLogger : ILogger
    {
        public List<LogEntry> LoggedMessages { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            LoggedMessages.Add(new LogEntry
            {
                LogLevel = logLevel,
                Message = formatter(state, exception)
            });
        }

        public class LogEntry
        {
            public LogLevel LogLevel { get; set; }
            public string Message { get; set; } = string.Empty;
        }
    }
}