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
    /// Note: Pipeline behaviors are NOT automatically registered. Use AddPipelineBehavior or AddPipelineBehaviors methods
    /// to register pipeline behaviors in the desired execution order.
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
    /// Registers a single pipeline behavior with the specified service lifetime.
    /// Behaviors are executed in the order they are registered.
    /// </summary>
    /// <typeparam name="TBehavior">The pipeline behavior type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceLifetime">The service lifetime for the behavior. Default is Transient.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPipelineBehavior<TBehavior>(this IServiceCollection services, 
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TBehavior : class
    {
        // Get all the IPipelineBehavior interfaces that this type implements
        var behaviorInterfaces = typeof(TBehavior)
            .GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));

        foreach (var behaviorInterface in behaviorInterfaces)
        {
            services.Add(new ServiceDescriptor(behaviorInterface, typeof(TBehavior), serviceLifetime));
        }

        return services;
    }

    /// <summary>
    /// Registers multiple pipeline behaviors in the specified order with the specified service lifetime.
    /// Behaviors are executed in the order they are provided in the array.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="behaviorTypes">The pipeline behavior types to register in execution order.</param>
    /// <param name="serviceLifetime">The service lifetime for the behaviors. Default is Transient.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPipelineBehaviors(this IServiceCollection services, 
        Type[] behaviorTypes,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (behaviorTypes == null)
            throw new ArgumentNullException(nameof(behaviorTypes));

        foreach (var behaviorType in behaviorTypes)
        {
            if (!behaviorType.IsClass)
                throw new ArgumentException($"Behavior type {behaviorType.Name} must be a class.", nameof(behaviorTypes));

            // Get all the IPipelineBehavior interfaces that this type implements
            var behaviorInterfaces = behaviorType
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));

            if (!behaviorInterfaces.Any())
                throw new ArgumentException($"Behavior type {behaviorType.Name} does not implement IPipelineBehavior<,>.", nameof(behaviorTypes));

            foreach (var behaviorInterface in behaviorInterfaces)
            {
                services.Add(new ServiceDescriptor(behaviorInterface, behaviorType, serviceLifetime));
            }
        }

        return services;
    }

    /// <summary>
    /// Registers the UseCase dispatcher and automatically discovers and registers all UseCases in the current assembly.
    /// Note: Pipeline behaviors are NOT automatically registered. Use AddPipelineBehavior or AddPipelineBehaviors methods
    /// to register pipeline behaviors in the desired execution order.
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
    /// Note: Pipeline behaviors are NOT automatically registered. Use AddPipelineBehavior or AddPipelineBehaviors methods
    /// to register pipeline behaviors in the desired execution order.
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