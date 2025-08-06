namespace FunctionalUseCases.Tests;

public class ExecutionResultTests
{
    [Fact]
    public void ExecutionResult_Success_ShouldReturnSuccessfulResult()
    {
        // Act
        var result = Execution.Success();

        // Assert
        Assert.True(result.ExecutionSucceeded);
        Assert.False(result.ExecutionFailed);
        Assert.Null(result.Error);
    }

    [Fact]
    public void ExecutionResult_Failure_ShouldReturnFailedResult()
    {
        // Arrange
        const string errorMessage = "Test error";

        // Act
        var result = Execution.Failure(errorMessage);

        // Assert
        Assert.False(result.ExecutionSucceeded);
        Assert.True(result.ExecutionFailed);
        Assert.NotNull(result.Error);
        Assert.Equal(errorMessage, result.Error.Message);
    }

    [Fact]
    public void ExecutionResult_ThrowIfFailed_ShouldThrowWhenFailed()
    {
        // Arrange
        var result = Execution.Failure("Test error");

        // Act & Assert
        var exception = Assert.Throws<ExecutionException>(() => result.ThrowIfFailed());
        Assert.Contains("Test error", exception.Message);
    }

    [Fact]
    public void ExecutionResult_ThrowIfFailed_ShouldNotThrowWhenSuccessful()
    {
        // Arrange
        var result = Execution.Success();

        // Act & Assert (no exception should be thrown)
        result.ThrowIfFailed();
    }
}