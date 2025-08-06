using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FunctionalUseCases.UseCases;

/// <summary>
/// Extension methods for registering use case handlers with dependency injection.
/// </summary>
public static class UseCaseRegistrationExtensions
{
    /// <summary>
    /// Registers all use case handlers from the specified assemblies using Scrutor.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for handlers. If none provided, scans the calling assembly.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddUseCases(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        // Register the dispatcher
        services.AddScoped<UseCaseDispatcher>();

        // Register all use case handlers using Scrutor
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo(typeof(IUseCaseHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    /// <summary>
    /// Registers all use case handlers from the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="TAssemblyMarker">A type from the assembly to scan.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddUseCasesFromAssemblyContaining<TAssemblyMarker>(this IServiceCollection services)
    {
        return services.AddUseCases(typeof(TAssemblyMarker).Assembly);
    }

    /// <summary>
    /// Registers all use case handlers from the assembly containing the specified type.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblyMarkerType">A type from the assembly to scan.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddUseCasesFromAssemblyContaining(this IServiceCollection services, Type assemblyMarkerType)
    {
        return services.AddUseCases(assemblyMarkerType.Assembly);
    }
}