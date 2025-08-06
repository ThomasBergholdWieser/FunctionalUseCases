using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FunctionalUseCases.UseCases.Core;

/// <summary>
/// Extension methods for registering use case handlers and the dispatcher.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the use case dispatcher and automatically scans for and registers all use case handlers in the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for handlers. If none provided, scans the calling assembly.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUseCases(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        // Register the dispatcher
        services.AddScoped<IUseCaseDispatcher, UseCaseDispatcher>();

        // Use Scrutor to automatically register handlers
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo(typeof(IUseCaseHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo(typeof(IUseCaseHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    /// <summary>
    /// Registers the use case dispatcher and automatically scans for and registers all use case handlers in the specified assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly to scan for handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUseCases(this IServiceCollection services, Assembly assembly)
    {
        return services.AddUseCases(new[] { assembly });
    }

    /// <summary>
    /// Registers the use case dispatcher and automatically scans for and registers all use case handlers in the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">A type from the assembly to scan.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUseCasesFromAssemblyContaining<T>(this IServiceCollection services)
    {
        return services.AddUseCases(typeof(T).Assembly);
    }
}