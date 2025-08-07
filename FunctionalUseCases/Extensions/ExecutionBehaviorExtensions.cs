using Microsoft.Extensions.DependencyInjection;

namespace FunctionalUseCases.Extensions;

/// <summary>
/// Extension methods for adding per-call execution behaviors to use case execution.
/// </summary>
public static class ExecutionBehaviorExtensions
{
    /// <summary>
    /// Creates an execution context with a specific behavior for per-call execution.
    /// This allows behaviors to be applied to specific use case executions rather than globally.
    /// </summary>
    /// <typeparam name="TBehavior">The type of behavior to add.</typeparam>
    /// <param name="dispatcher">The use case dispatcher.</param>
    /// <returns>An execution context with the specified behavior.</returns>
    public static IExecutionContext WithBehavior<TBehavior>(this IUseCaseDispatcher dispatcher)
        where TBehavior : class
    {
        if (dispatcher == null)
        {
            throw new ArgumentNullException(nameof(dispatcher));
        }

        // We need access to the service provider to resolve behaviors
        // Since UseCaseDispatcher takes IServiceProvider in constructor, we'll need to get it
        var serviceProvider = GetServiceProvider(dispatcher);
        var context = new ExecutionContext(dispatcher, serviceProvider);
        return context.WithBehavior<TBehavior>();
    }

    /// <summary>
    /// Creates an execution context with a specific behavior instance for per-call execution.
    /// </summary>
    /// <param name="dispatcher">The use case dispatcher.</param>
    /// <param name="behavior">The behavior instance to add.</param>
    /// <returns>An execution context with the specified behavior.</returns>
    public static IExecutionContext WithBehavior(this IUseCaseDispatcher dispatcher, object behavior)
    {
        if (dispatcher == null)
        {
            throw new ArgumentNullException(nameof(dispatcher));
        }

        if (behavior == null)
        {
            throw new ArgumentNullException(nameof(behavior));
        }

        var serviceProvider = GetServiceProvider(dispatcher);
        var context = new ExecutionContext(dispatcher, serviceProvider);
        return context.WithBehavior(behavior);
    }

    /// <summary>
    /// Creates a typed execution context with a specific behavior for per-call execution.
    /// </summary>
    /// <typeparam name="TResult">The result type of the execution.</typeparam>
    /// <typeparam name="TBehavior">The type of behavior to add.</typeparam>
    /// <param name="dispatcher">The use case dispatcher.</param>
    /// <returns>A typed execution context with the specified behavior.</returns>
    public static IExecutionContext<TResult> WithBehavior<TResult, TBehavior>(this IUseCaseDispatcher dispatcher)
        where TResult : notnull
        where TBehavior : class
    {
        if (dispatcher == null)
        {
            throw new ArgumentNullException(nameof(dispatcher));
        }

        var serviceProvider = GetServiceProvider(dispatcher);
        var context = new ExecutionContext<TResult>(dispatcher, serviceProvider);
        return context.WithBehavior<TBehavior>();
    }

    /// <summary>
    /// Creates a typed execution context with a specific behavior instance for per-call execution.
    /// </summary>
    /// <typeparam name="TResult">The result type of the execution.</typeparam>
    /// <param name="dispatcher">The use case dispatcher.</param>
    /// <param name="behavior">The behavior instance to add.</param>
    /// <returns>A typed execution context with the specified behavior.</returns>
    public static IExecutionContext<TResult> WithBehavior<TResult>(this IUseCaseDispatcher dispatcher, object behavior)
        where TResult : notnull
    {
        if (dispatcher == null)
        {
            throw new ArgumentNullException(nameof(dispatcher));
        }

        if (behavior == null)
        {
            throw new ArgumentNullException(nameof(behavior));
        }

        var serviceProvider = GetServiceProvider(dispatcher);
        var context = new ExecutionContext<TResult>(dispatcher, serviceProvider);
        return context.WithBehavior(behavior);
    }

    private static IServiceProvider GetServiceProvider(IUseCaseDispatcher dispatcher)
    {
        return dispatcher.ServiceProvider;
    }
}