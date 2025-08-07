using Microsoft.Extensions.DependencyInjection;

namespace FunctionalUseCases.Tests;

public class UseCaseDispatcherTests
{
    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UseCaseDispatcher(null!));
    }

    [Fact]
    public async Task ExecuteAsync_WithNullParameter_ShouldThrowArgumentNullException()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            dispatcher.ExecuteAsync<string>(null!));
    }

    [Fact]
    public async Task ExecuteAsync_WithUnregisteredUseCase_ShouldReturnFailure()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);
        var parameter = new TestUseCaseParameter();

        // Act
        var result = await dispatcher.ExecuteAsync<string>(parameter);

        // Assert
        Assert.False(result.ExecutionSucceeded);
        Assert.True(result.ExecutionFailed);
        Assert.NotNull(result.Error);
        Assert.Contains("No use case registered", result.Error.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithRegisteredUseCase_ShouldExecuteSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IUseCase<TestUseCaseParameter, string>, TestUseCase>();
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);
        var parameter = new TestUseCaseParameter();

        // Act
        var result = await dispatcher.ExecuteAsync<string>(parameter);

        // Assert
        Assert.True(result.ExecutionSucceeded);
        Assert.Equal("Test Result", result.CheckedValue);
    }

    [Fact]
    public async Task ExecuteAsync_WithBehavior_ShouldExecuteThroughBehavior()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IUseCase<TestUseCaseParameter, string>, TestUseCase>();
        services.AddTransient<IExecutionBehavior<TestUseCaseParameter, string>, TestBehavior>();
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);
        var parameter = new TestUseCaseParameter();

        // Act
        var result = await dispatcher.ExecuteAsync<string>(parameter);

        // Assert
        Assert.True(result.ExecutionSucceeded);
        Assert.Equal("Behavior: Test Result", result.CheckedValue);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleBehaviors_ShouldExecuteInOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IUseCase<TestUseCaseParameter, string>, TestUseCase>();
        services.AddTransient<IExecutionBehavior<TestUseCaseParameter, string>, TestBehavior>();
        services.AddTransient<IExecutionBehavior<TestUseCaseParameter, string>, TestBehavior2>();
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);
        var parameter = new TestUseCaseParameter();

        // Act
        var result = await dispatcher.ExecuteAsync<string>(parameter);

        // Assert
        Assert.True(result.ExecutionSucceeded);
        // Due to reverse order wrapping in the pipeline, TestBehavior executes first, then TestBehavior2
        Assert.Equal("Behavior: Behavior2: Test Result", result.CheckedValue);
    }

    // Test helper classes
    public class TestUseCaseParameter : IUseCaseParameter<string>
    {
    }

    public class TestUseCase : IUseCase<TestUseCaseParameter, string>
    {
        public Task<ExecutionResult<string>> ExecuteAsync(TestUseCaseParameter useCaseParameter, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Execution.Success("Test Result"));
        }
    }

    public class TestBehavior : IExecutionBehavior<TestUseCaseParameter, string>
    {
        public async Task<ExecutionResult<string>> ExecuteAsync(TestUseCaseParameter useCaseParameter, PipelineBehaviorDelegate<string> next, CancellationToken cancellationToken = default)
        {
            var result = await next();
            if (result.ExecutionSucceeded)
            {
                return Execution.Success("Behavior: " + result.CheckedValue);
            }
            return result;
        }
    }

    public class TestBehavior2 : IExecutionBehavior<TestUseCaseParameter, string>
    {
        public async Task<ExecutionResult<string>> ExecuteAsync(TestUseCaseParameter useCaseParameter, PipelineBehaviorDelegate<string> next, CancellationToken cancellationToken = default)
        {
            var result = await next();
            if (result.ExecutionSucceeded)
            {
                return Execution.Success("Behavior2: " + result.CheckedValue);
            }
            return result;
        }
    }
}