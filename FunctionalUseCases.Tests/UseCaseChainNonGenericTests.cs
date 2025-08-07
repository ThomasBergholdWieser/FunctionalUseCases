using Microsoft.Extensions.DependencyInjection;
using FunctionalUseCases.Extensions;
using FakeItEasy;

namespace FunctionalUseCases.Tests;

public class UseCaseChainNonGenericTests
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
    public void Chain_EmptyChain_ShouldStartWithNonGenericChain()
    {
        // Arrange
        var dispatcher = CreateMockDispatcher();

        // Act
        var chain = dispatcher.StartWith();

        // Assert
        chain.ShouldNotBeNull();
        chain.ShouldBeOfType<UseCaseChain>();
    }

    [Fact]
    public async Task Chain_NonGenericChainThenUseCase_ShouldBecomeTypedChain()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IUseCase<VoidUseCaseParameter, string>, VoidUseCase>();
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);

        var typedChain = dispatcher
            .StartWith()
            .Then(new VoidUseCaseParameter());

        // Act
        var result = await typedChain.ExecuteAsync();

        // Assert
        result.ExecutionSucceeded.ShouldBeTrue();
        result.CheckedValue.ShouldBe("Void Result");
    }

    [Fact]
    public async Task Chain_NonGenericChainWithErrorHandling_ShouldWorkWithTypedChain()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IUseCase<FailingVoidParameter, string>, FailingVoidUseCase>();
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new UseCaseDispatcher(serviceProvider);

        var errorHandlerCalled = false;

        var chain = dispatcher
            .StartWith()
            .Then(new FailingVoidParameter())
            .OnError(error =>
            {
                errorHandlerCalled = true;
                return Task.FromResult(Execution.Success("Error handled"));
            });

        // Act
        var result = await chain.ExecuteAsync();

        // Assert
        result.ExecutionSucceeded.ShouldBeTrue();
        result.CheckedValue.ShouldBe("Error handled");
        errorHandlerCalled.ShouldBeTrue();
    }

    // Test helper classes
    public class VoidUseCaseParameter : IUseCaseParameter<string> { }

    public class VoidUseCase : IUseCase<VoidUseCaseParameter, string>
    {
        public Task<ExecutionResult<string>> ExecuteAsync(VoidUseCaseParameter useCaseParameter, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Execution.Success("Void Result"));
        }
    }

    public class FailingVoidParameter : IUseCaseParameter<string> { }

    public class FailingVoidUseCase : IUseCase<FailingVoidParameter, string>
    {
        public Task<ExecutionResult<string>> ExecuteAsync(FailingVoidParameter useCaseParameter, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Execution.Failure<string>("Void failure"));
        }
    }
}