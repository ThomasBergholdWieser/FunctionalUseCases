using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FunctionalUseCases;

/// <summary>
/// Extension methods for registering UseCases and dispatcher using Scrutor.
/// </summary>
public static class UseCaseRegistrationExtensions
{
    /// <summary>
    /// Registers the UseCase dispatcher and automatically discovers and registers all UseCases in the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for UseCases. If null, scans the calling assembly.</param>
    /// <param name="serviceLifetime">The service lifetime for the use cases. Default is Transient.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUseCases(this IServiceCollection services, 
        Assembly[]? assemblies = null, 
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        // Register the dispatcher
        services.AddTransient<IUseCaseDispatcher, UseCaseDispatcher>();

        // Default to calling assembly if none specified
        assemblies ??= new[] { Assembly.GetCallingAssembly() };

        // Register all UseCases using Scrutor
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo(typeof(IUseCase<,>)))
            .AsImplementedInterfaces()
            .WithLifetime(serviceLifetime));

        return services;
    }

    /// <summary>
    /// Registers the UseCase dispatcher and automatically discovers and registers all UseCases in the current assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceLifetime">The service lifetime for the use cases. Default is Transient.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUseCasesFromAssembly(this IServiceCollection services, 
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        return services.AddUseCases(new[] { Assembly.GetCallingAssembly() }, serviceLifetime);
    }

    /// <summary>
    /// Registers the UseCase dispatcher and automatically discovers and registers all UseCases in the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">A type from the assembly to scan.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceLifetime">The service lifetime for the use cases. Default is Transient.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUseCasesFromAssemblyContaining<T>(this IServiceCollection services, 
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        return services.AddUseCases(new[] { typeof(T).Assembly }, serviceLifetime);
    }
}