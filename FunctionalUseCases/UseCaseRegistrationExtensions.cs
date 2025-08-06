using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FunctionalUseCases;

/// <summary>
/// Extension methods for registering UseCase handlers and dispatcher using Scrutor.
/// </summary>
public static class UseCaseRegistrationExtensions
{
    /// <summary>
    /// Registers the UseCase dispatcher and automatically discovers and registers all UseCase handlers in the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for UseCase handlers. If null, scans the calling assembly.</param>
    /// <param name="serviceLifetime">The service lifetime for the handlers. Default is Scoped.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUseCases(this IServiceCollection services, 
        Assembly[]? assemblies = null, 
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        // Register the dispatcher
        services.AddScoped<IUseCaseDispatcher, UseCaseDispatcher>();

        // Default to calling assembly if none specified
        assemblies ??= new[] { Assembly.GetCallingAssembly() };

        // Register all UseCase handlers using Scrutor
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo(typeof(IUseCaseHandler<,>)))
            .AsImplementedInterfaces()
            .WithLifetime(serviceLifetime));

        return services;
    }

    /// <summary>
    /// Registers the UseCase dispatcher and automatically discovers and registers all UseCase handlers in the current assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceLifetime">The service lifetime for the handlers. Default is Scoped.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUseCasesFromAssembly(this IServiceCollection services, 
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        return services.AddUseCases(new[] { Assembly.GetCallingAssembly() }, serviceLifetime);
    }

    /// <summary>
    /// Registers the UseCase dispatcher and automatically discovers and registers all UseCase handlers in the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">A type from the assembly to scan.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceLifetime">The service lifetime for the handlers. Default is Scoped.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUseCasesFromAssemblyContaining<T>(this IServiceCollection services, 
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        return services.AddUseCases(new[] { typeof(T).Assembly }, serviceLifetime);
    }
}