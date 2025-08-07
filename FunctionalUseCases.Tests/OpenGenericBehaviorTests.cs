using Microsoft.Extensions.DependencyInjection;
using FunctionalUseCases.Extensions;

namespace FunctionalUseCases.Tests;

public class OpenGenericBehaviorTests
{
    [Fact]
    public async Task WithBehavior_WithOpenGenericType_ShouldResolveAndExecuteBehavior()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IUseCase<TestUseCaseParameter, string>, TestUseCase>();
        services.AddTransient(typeof(GenericTestBehavior<,>)); // Register as open generic
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);
        var parameter = new TestUseCaseParameter();

        // Act
        var result = await dispatcher
            .WithBehavior(typeof(GenericTestBehavior<,>))
            .ExecuteAsync(parameter);

        // Assert
        result.ExecutionSucceeded.ShouldBeTrue();
        result.CheckedValue.ShouldBe("Behavior: Test Result");
    }

    [Fact]
    public async Task WithBehavior_WithTypedContext_WithOpenGenericType_ShouldResolveAndExecuteBehavior()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IUseCase<TestUseCaseParameter, string>, TestUseCase>();
        services.AddTransient(typeof(GenericTestBehavior<,>)); // Register as open generic
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);
        var parameter = new TestUseCaseParameter();

        // Act
        var result = await dispatcher
            .WithBehavior<string>(typeof(GenericTestBehavior<,>))
            .ExecuteAsync(parameter);

        // Assert
        result.ExecutionSucceeded.ShouldBeTrue();
        result.CheckedValue.ShouldBe("Behavior: Test Result");
    }

    [Fact]
    public void WithBehavior_WithConcreteType_ShouldThrowArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            dispatcher.WithBehavior(typeof(GenericTestBehavior<TestUseCaseParameter, string>)))
            .Message.ShouldContain("open generic type definition");
    }

    [Fact]
    public void WithBehavior_WithNullType_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            dispatcher.WithBehavior((Type)null!));
    }

    [Fact]
    public async Task WithBehavior_WithOpenGenericType_UnregisteredBehavior_ShouldReturnFailure()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IUseCase<TestUseCaseParameter, string>, TestUseCase>();
        // Note: Not registering the behavior
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);
        var parameter = new TestUseCaseParameter();

        // Act
        var result = await dispatcher
            .WithBehavior(typeof(GenericTestBehavior<,>))
            .ExecuteAsync(parameter);

        // Assert
        result.ExecutionSucceeded.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Message.ShouldContain("Failed to resolve open generic behavior");
    }

    [Fact]
    public async Task WithBehavior_WithMultipleOpenGenericBehaviors_ShouldExecuteInOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IUseCase<TestUseCaseParameter, string>, TestUseCase>();
        services.AddTransient(typeof(GenericTestBehavior<,>));
        services.AddTransient(typeof(SecondGenericTestBehavior<,>));
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);
        var parameter = new TestUseCaseParameter();

        // Act
        var result = await dispatcher
            .WithBehavior(typeof(GenericTestBehavior<,>))
            .WithBehavior(typeof(SecondGenericTestBehavior<,>))
            .ExecuteAsync(parameter);

        // Assert
        result.ExecutionSucceeded.ShouldBeTrue();
        // Due to reverse order wrapping in the pipeline, GenericTestBehavior executes first, then SecondGenericTestBehavior
        result.CheckedValue.ShouldBe("Behavior: SecondBehavior: Test Result");
    }

    [Fact]
    public async Task UseCaseChain_WithBehavior_WithOpenGenericType_ShouldExecuteBehaviorInChain()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IUseCase<TestUseCaseParameter, string>, TestUseCase>();
        services.AddTransient(typeof(GenericTestBehavior<,>));
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);

        // Act
        var result = await dispatcher
            .StartWith(new TestUseCaseParameter())
            .WithBehavior(typeof(GenericTestBehavior<,>))
            .Then(new TestUseCaseParameter())
            .ExecuteAsync();

        // Assert
        result.ExecutionSucceeded.ShouldBeTrue();
        result.CheckedValue.ShouldBe("Behavior: Test Result");
    }

    [Fact]
    public async Task ExecutionContext_WithBehavior_WithOpenGenericType_ShouldResolveCorrectConcreteType()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IUseCase<TestUseCaseParameter, string>, TestUseCase>();
        services.AddTransient<IUseCase<AnotherTestUseCaseParameter, int>, AnotherTestUseCase>();
        services.AddTransient(typeof(TypeCapturingBehavior<,>));
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);

        // Act - First use case with string result
        var stringResult = await dispatcher
            .WithBehavior(typeof(TypeCapturingBehavior<,>))
            .ExecuteAsync(new TestUseCaseParameter());

        // Act - Second use case with int result  
        var intResult = await dispatcher
            .WithBehavior(typeof(TypeCapturingBehavior<,>))
            .ExecuteAsync(new AnotherTestUseCaseParameter());

        // Assert
        stringResult.ExecutionSucceeded.ShouldBeTrue();
        stringResult.CheckedValue.ShouldBe("TypeCapturing<TestUseCaseParameter,String>: Test Result");

        intResult.ExecutionSucceeded.ShouldBeTrue();
        intResult.CheckedValue.ShouldBe(42);
    }
}

// Test behaviors for the new tests - these need to be generic
public class GenericTestBehavior<TUseCaseParameter, TResult> : IExecutionBehavior<TUseCaseParameter, TResult>
    where TUseCaseParameter : IUseCaseParameter<TResult>
    where TResult : notnull
{
    public async Task<ExecutionResult<TResult>> ExecuteAsync(TUseCaseParameter useCaseParameter, PipelineBehaviorDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        var result = await next().ConfigureAwait(false);

        if (result.ExecutionSucceeded && result.CheckedValue is string value)
        {
            return Execution.Success((TResult)(object)$"Behavior: {value}");
        }

        return result;
    }
}

public class SecondGenericTestBehavior<TUseCaseParameter, TResult> : IExecutionBehavior<TUseCaseParameter, TResult>
    where TUseCaseParameter : IUseCaseParameter<TResult>
    where TResult : notnull
{
    public async Task<ExecutionResult<TResult>> ExecuteAsync(TUseCaseParameter useCaseParameter, PipelineBehaviorDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        var result = await next().ConfigureAwait(false);

        if (result.ExecutionSucceeded && result.CheckedValue is string value)
        {
            return Execution.Success((TResult)(object)$"SecondBehavior: {value}");
        }

        return result;
    }
}

public class TypeCapturingBehavior<TUseCaseParameter, TResult> : IExecutionBehavior<TUseCaseParameter, TResult>
    where TUseCaseParameter : IUseCaseParameter<TResult>
    where TResult : notnull
{
    public async Task<ExecutionResult<TResult>> ExecuteAsync(TUseCaseParameter useCaseParameter, PipelineBehaviorDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        var result = await next().ConfigureAwait(false);

        if (result.ExecutionSucceeded && result.CheckedValue is string value)
        {
            var parameterTypeName = typeof(TUseCaseParameter).Name;
            var resultTypeName = typeof(TResult).Name;
            return Execution.Success((TResult)(object)$"TypeCapturing<{parameterTypeName},{resultTypeName}>: {value}");
        }

        return result;
    }
}

// Additional test use case for different result type
public class AnotherTestUseCaseParameter : IUseCaseParameter<int>
{
}

public class AnotherTestUseCase : IUseCase<AnotherTestUseCaseParameter, int>
{
    public Task<ExecutionResult<int>> ExecuteAsync(AnotherTestUseCaseParameter useCaseParameter, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Execution.Success(42));
    }
}

// Test types that need to exist
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