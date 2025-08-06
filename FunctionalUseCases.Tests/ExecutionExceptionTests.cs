namespace FunctionalUseCases.Tests;

public class ExecutionExceptionTests
{
    [Fact]
    public void ExecutionException_Constructor_ShouldSetMessage()
    {
        // Arrange
        const string message = "Test exception message";

        // Act
        var exception = new ExecutionException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void ExecutionException_ShouldBeSerializable()
    {
        // Arrange
        const string message = "Test exception message";
        var exception = new ExecutionException(message);

        // Act & Assert - Should not throw
        // The [Serializable] attribute is applied to the class
        Assert.True(exception.GetType().GetCustomAttributes(typeof(SerializableAttribute), false).Length > 0);
    }

    [Fact]
    public void ExecutionException_ShouldInheritFromException()
    {
        // Arrange
        var exception = new ExecutionException("test");

        // Act & Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }
}