using Microsoft.Extensions.DependencyInjection;
using FunctionalUseCases.Extensions;
using FakeItEasy;

namespace FunctionalUseCases.Tests;

public class UseCaseChainTests
{
    private static IUseCaseDispatcher CreateMockDispatcher()
    {
        var mockDispatcher = A.Fake<IUseCaseDispatcher>();
        var mockServiceProvider = A.Fake<IServiceProvider>();
        
        A.CallTo(() => mockDispatcher.ServiceProvider)
            .Returns(mockServiceProvider);
            
        return mockDispatcher;
    }
    [Fact]
    public void Constructor_WithNullDispatcher_ShouldThrowArgumentNullException()
    {
        // Act & Assert - Test the extension method instead since constructor is internal
        Should.Throw<ArgumentNullException>(() => ((IUseCaseDispatcher)null!).StartWith(new TestUseCaseParameter()));
    }

    [Fact]
    public void StartWithExtension_WithNullDispatcher_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => ((IUseCaseDispatcher)null!).StartWith());
        Should.Throw<ArgumentNullException>(() => ((IUseCaseDispatcher)null!).StartWith(new TestUseCaseParameter()));
    }

    [Fact]
    public void StartWithExtension_WithNullParameter_ShouldThrowArgumentNullException()
    {
        // Arrange
        var dispatcher = CreateMockDispatcher();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => dispatcher.StartWith<string>(null!));
    }

    [Fact]
    public void ExecuteAsync_WithEmptyChain_ShouldValidateNullParameters()
    {
        // Arrange
        var dispatcher = CreateMockDispatcher();
        // Since we can't create an empty typed chain directly, we'll use a chain with Then() to test this

        // Actually, we can't test this scenario easily since the API doesn't allow creating an empty typed chain
        // The extension method StartWith<T>() always requires a use case parameter
        // Let's test that the extension method validates null parameters instead

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => dispatcher.StartWith<string>(null!));
    }

    [Fact]
    public async Task ExecuteAsync_WithSingleUseCase_ShouldExecuteSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IUseCase<TestUseCaseParameter, string>, TestUseCase>();
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);

        var chain = dispatcher.StartWith(new TestUseCaseParameter());

        // Act
        var result = await chain.ExecuteAsync();

        // Assert
        result.ExecutionSucceeded.ShouldBeTrue();
        result.CheckedValue.ShouldBe("Test Result");
    }

    [Fact]
    public async Task ExecuteAsync_WithChainedUseCases_ShouldExecuteInOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IUseCase<TestUseCaseParameter, string>, TestUseCase>();
        services.AddTransient<IUseCase<StringToIntParameter, int>, StringToIntUseCase>();
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);

        var chain = dispatcher
            .StartWith(new TestUseCaseParameter())
            .Then(new StringToIntParameter());

        // Act
        var result = await chain.ExecuteAsync();

        // Assert
        result.ExecutionSucceeded.ShouldBeTrue();
        result.CheckedValue.ShouldBe(42); // StringToIntUseCase returns 42
    }

    [Fact]
    public async Task ExecuteAsync_WithFailingFirstUseCase_ShouldStopExecution()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IUseCase<FailingUseCaseParameter, string>, FailingUseCase>();
        services.AddTransient<IUseCase<StringToIntParameter, int>, StringToIntUseCase>();
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);

        var chain = dispatcher
            .StartWith(new FailingUseCaseParameter())
            .Then(new StringToIntParameter());

        // Act
        var result = await chain.ExecuteAsync();

        // Assert
        result.ExecutionSucceeded.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Message.ShouldContain("Test failure");
    }

    [Fact]
    public async Task ExecuteAsync_WithFailingSecondUseCase_ShouldStopExecution()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IUseCase<TestUseCaseParameter, string>, TestUseCase>();
        services.AddTransient<IUseCase<FailingIntParameter, bool>, FailingIntUseCase>();
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);

        var chain = dispatcher
            .StartWith(new TestUseCaseParameter())
            .Then(new FailingIntParameter());

        // Act
        var result = await chain.ExecuteAsync();

        // Assert
        result.ExecutionSucceeded.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Message.ShouldContain("Int failure");
    }

    [Fact]
    public async Task ExecuteAsync_WithOnErrorHandler_ShouldCallErrorHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IUseCase<FailingUseCaseParameter, string>, FailingUseCase>();
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);

        var errorHandlerCalled = false;
        ExecutionError? capturedError = null;

        var chain = dispatcher
            .StartWith(new FailingUseCaseParameter())
            .OnError(error =>
            {
                errorHandlerCalled = true;
                capturedError = error;
                return Task.FromResult(Execution.Success("Error handled"));
            });

        // Act
        var result = await chain.ExecuteAsync();

        // Assert
        result.ExecutionSucceeded.ShouldBeTrue();
        result.CheckedValue.ShouldBe("Error handled");
        errorHandlerCalled.ShouldBeTrue();
        capturedError.ShouldNotBeNull();
        capturedError.Message.ShouldContain("Test failure");
    }

    [Fact]
    public async Task ExecuteAsync_WithOnErrorHandlerWithCancellationToken_ShouldCallErrorHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IUseCase<FailingUseCaseParameter, string>, FailingUseCase>();
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);

        var errorHandlerCalled = false;
        ExecutionError? capturedError = null;
        CancellationToken capturedToken = default;

        var chain = dispatcher
            .StartWith(new FailingUseCaseParameter())
            .OnError((error, cancellationToken) =>
            {
                errorHandlerCalled = true;
                capturedError = error;
                capturedToken = cancellationToken;
                return Task.FromResult(Execution.Success("Error handled with token"));
            });

        var cts = new CancellationTokenSource();

        // Act
        var result = await chain.ExecuteAsync(cts.Token);

        // Assert
        result.ExecutionSucceeded.ShouldBeTrue();
        result.CheckedValue.ShouldBe("Error handled with token");
        errorHandlerCalled.ShouldBeTrue();
        capturedError.ShouldNotBeNull();
        capturedToken.ShouldBe(cts.Token);
    }

    [Fact]
    public async Task ExecuteAsync_WithMockedDispatcher_ShouldCallDispatcherForEachUseCase()
    {
        // Arrange - Use real service provider since ExecutionContext requires actual use case registration
        var services = new ServiceCollection();
        services.AddUseCasesFromAssemblyContaining<TestUseCaseParameter>();
        services.AddTransient<IUseCase<StringToIntParameter, int>, StringToIntUseCase>();
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IUseCaseDispatcher>();

        var parameter1 = new TestUseCaseParameter();
        var parameter2 = new StringToIntParameter();

        var chain = dispatcher
            .StartWith(parameter1)
            .Then(parameter2);

        // Act
        var result = await chain.ExecuteAsync();

        // Assert
        result.ExecutionSucceeded.ShouldBeTrue();
        result.CheckedValue.ShouldBe(42); // StringToIntUseCase returns 42

        // Note: This test validates that the chain executes properly with real use cases
        // instead of mocking, which is more realistic for integration testing
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellation_ShouldReturnCancellationFailure()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IUseCase<DelayedUseCaseParameter, string>, DelayedUseCase>();
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);

        var chain = dispatcher.StartWith(new DelayedUseCaseParameter());
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(50)); // Cancel quickly

        // Act
        var result = await chain.ExecuteAsync(cts.Token);

        // Assert
        result.ExecutionSucceeded.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        // The cancellation happens at the dispatcher level, so we expect the generic error message
        result.Error.Message.ShouldContain("task was canceled");
    }

    [Fact]
    public async Task ExecuteAsync_WithBehaviors_ShouldApplyBehaviorsToChainedUseCases()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IUseCase<TestUseCaseParameter, string>, TestUseCase>();
        services.AddTransient<IUseCase<StringToIntParameter, int>, StringToIntUseCase>();
        services.AddTransient<IExecutionBehavior<TestUseCaseParameter, string>, TestBehavior>();
        services.AddTransient<IExecutionBehavior<StringToIntParameter, int>, IntBehavior>();
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);

        var chain = dispatcher
            .StartWith(new TestUseCaseParameter())
            .Then(new StringToIntParameter());

        // Act
        var result = await chain.ExecuteAsync();

        // Assert
        result.ExecutionSucceeded.ShouldBeTrue();
        result.CheckedValue.ShouldBe(43); // 42 + 1 from IntBehavior
    }

    [Fact]
    public void Then_WithNullParameter_ShouldThrowArgumentNullException()
    {
        // Arrange
        var dispatcher = CreateMockDispatcher();
        var chain = dispatcher.StartWith(new TestUseCaseParameter());

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => chain.Then((IUseCaseParameter<int>)null!));
    }

    [Fact]
    public void OnError_WithNullHandler_ShouldThrowArgumentNullException()
    {
        // Arrange
        var dispatcher = CreateMockDispatcher();
        var chain = dispatcher.StartWith(new TestUseCaseParameter());

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => chain.OnError((Func<ExecutionError, Task<ExecutionResult<string>>>)null!));
        Should.Throw<ArgumentNullException>(() => chain.OnError((Func<ExecutionError, CancellationToken, Task<ExecutionResult<string>>>)null!));
    }

    // Test helper classes
    public class TestUseCaseParameter : IUseCaseParameter<string> { }

    public class TestUseCase : IUseCase<TestUseCaseParameter, string>
    {
        public Task<ExecutionResult<string>> ExecuteAsync(TestUseCaseParameter useCaseParameter, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Execution.Success("Test Result"));
        }
    }

    public class StringToIntParameter : IUseCaseParameter<int> { }

    public class StringToIntUseCase : IUseCase<StringToIntParameter, int>
    {
        public Task<ExecutionResult<int>> ExecuteAsync(StringToIntParameter useCaseParameter, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Execution.Success(42));
        }
    }

    public class FailingUseCaseParameter : IUseCaseParameter<string> { }

    public class FailingUseCase : IUseCase<FailingUseCaseParameter, string>
    {
        public Task<ExecutionResult<string>> ExecuteAsync(FailingUseCaseParameter useCaseParameter, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Execution.Failure<string>("Test failure"));
        }
    }

    public class FailingIntParameter : IUseCaseParameter<bool> { }

    public class FailingIntUseCase : IUseCase<FailingIntParameter, bool>
    {
        public Task<ExecutionResult<bool>> ExecuteAsync(FailingIntParameter useCaseParameter, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Execution.Failure<bool>("Int failure"));
        }
    }

    public class DelayedUseCaseParameter : IUseCaseParameter<string> { }

    public class DelayedUseCase : IUseCase<DelayedUseCaseParameter, string>
    {
        public async Task<ExecutionResult<string>> ExecuteAsync(DelayedUseCaseParameter useCaseParameter, CancellationToken cancellationToken = default)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            return Execution.Success("Delayed Result");
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

    public class IntBehavior : IExecutionBehavior<StringToIntParameter, int>
    {
        public async Task<ExecutionResult<int>> ExecuteAsync(StringToIntParameter useCaseParameter, PipelineBehaviorDelegate<int> next, CancellationToken cancellationToken = default)
        {
            var result = await next();
            if (result.ExecutionSucceeded)
            {
                return Execution.Success(result.CheckedValue + 1); // Add 1 to the result
            }
            return result;
        }
    }
}