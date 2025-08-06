using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FunctionalUseCases.Tests;

public class UseCaseRegistrationExtensionsTests
{
    [Fact]
    public void AddUseCases_ShouldRegisterDispatcher()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddUseCases();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dispatcher = serviceProvider.GetService<IUseCaseDispatcher>();
        Assert.NotNull(dispatcher);
        Assert.IsType<UseCaseDispatcher>(dispatcher);
    }

    [Fact]
    public void AddUseCases_WithAssemblies_ShouldRegisterUseCasesFromSpecifiedAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();
        var assemblies = new[] { Assembly.GetExecutingAssembly() };

        // Act
        services.AddUseCases(assemblies);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dispatcher = serviceProvider.GetService<IUseCaseDispatcher>();
        Assert.NotNull(dispatcher);

        // Check if test use case from this assembly is registered
        var useCase = serviceProvider.GetService<IUseCase<TestUseCaseParameter, string>>();
        Assert.NotNull(useCase);
    }

    [Fact]
    public void AddUseCases_WithServiceLifetime_ShouldRegisterWithCorrectLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddUseCases(serviceLifetime: ServiceLifetime.Singleton);

        // Assert
        var useCaseDescriptor = services.FirstOrDefault(s => 
            s.ServiceType.IsGenericType && 
            s.ServiceType.GetGenericTypeDefinition() == typeof(IUseCase<,>));
        
        if (useCaseDescriptor != null)
        {
            Assert.Equal(ServiceLifetime.Singleton, useCaseDescriptor.Lifetime);
        }
    }

    [Fact]
    public void AddUseCasesFromAssembly_ShouldRegisterDispatcherAndUseCases()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddUseCasesFromAssembly();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dispatcher = serviceProvider.GetService<IUseCaseDispatcher>();
        Assert.NotNull(dispatcher);
    }

    [Fact]
    public void AddUseCasesFromAssemblyContaining_ShouldRegisterUseCasesFromCorrectAssembly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddUseCasesFromAssemblyContaining<TestUseCaseParameter>();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dispatcher = serviceProvider.GetService<IUseCaseDispatcher>();
        Assert.NotNull(dispatcher);

        var useCase = serviceProvider.GetService<IUseCase<TestUseCaseParameter, string>>();
        Assert.NotNull(useCase);
    }

    [Fact]
    public void AddUseCases_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddUseCases();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddUseCasesFromAssembly_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddUseCasesFromAssembly();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddUseCasesFromAssemblyContaining_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddUseCasesFromAssemblyContaining<TestUseCaseParameter>();

        // Assert
        Assert.Same(services, result);
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
}