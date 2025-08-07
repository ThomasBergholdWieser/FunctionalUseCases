namespace FunctionalUseCases.Tests;

public class ExecutionResultGenericTests
{
    [Fact]
    public void ExecutionResult_Success_ShouldReturnSuccessfulResultWithValue()
    {
        // Arrange
        const string value = "test value";

        // Act
        var result = Execution.Success(value);

        // Assert
        result.ExecutionSucceeded.ShouldBeTrue();
        result.ExecutionFailed.ShouldBeFalse();
        result.Error.ShouldBeNull();
        result.CheckedValue.ShouldBe(value);
    }

    [Fact]
    public void ExecutionResult_Failure_ShouldReturnFailedResult()
    {
        // Arrange
        const string errorMessage = "Test error";

        // Act
        var result = Execution.Failure<string>(errorMessage);

        // Assert
        result.ExecutionSucceeded.ShouldBeFalse();
        result.ExecutionFailed.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.Message.ShouldBe(errorMessage);
    }

    [Fact]
    public void ExecutionResult_CheckedValue_ShouldThrowWhenFailed()
    {
        // Arrange
        var result = Execution.Failure<string>("Test error");

        // Act & Assert
        Should.Throw<NullReferenceException>(() => result.CheckedValue);
    }

    [Fact]
    public void ExecutionResult_ImplicitConversion_ShouldWorkFromValue()
    {
        // Arrange
        const int value = 42;

        // Act
        ExecutionResult<int> result = value;

        // Assert
        result.ExecutionSucceeded.ShouldBeTrue();
        result.CheckedValue.ShouldBe(value);
    }

    [Fact]
    public void ExecutionResult_ToString_ShouldShowAppropriateMessage()
    {
        // Arrange
        var successResult = Execution.Success("test");
        var failureResult = Execution.Failure<string>("error message");

        // Act & Assert
        successResult.ToString().ShouldBe("ExecutionSucceeded");
        failureResult.ToString().ShouldContain("ExecutionFailed");
        failureResult.ToString().ShouldContain("error message");
    }

    [Fact]
    public void ExecutionResult_Combine_ShouldReturnSuccessWhenAllSucceed()
    {
        // Arrange
        var result1 = Execution.Success("value1");
        var result2 = Execution.Success("value2");

        // Act
        var combined = result1 + result2;

        // Assert
        combined.ExecutionSucceeded.ShouldBeTrue();
    }

    [Fact]
    public void ExecutionResult_Combine_ShouldReturnFailureWhenAnyFails()
    {
        // Arrange
        var successResult = Execution.Success("value");
        var failureResult = Execution.Failure<string>("error");

        // Act
        var combined = successResult + failureResult;

        // Assert
        combined.ExecutionFailed.ShouldBeTrue();
    }
}