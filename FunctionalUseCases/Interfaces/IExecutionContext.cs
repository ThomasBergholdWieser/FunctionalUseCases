namespace FunctionalUseCases;

/// <summary>
/// Represents an execution context that can hold per-call execution behaviors.
/// This allows behaviors to be applied to specific use case executions or chains
/// rather than being globally registered.
/// </summary>
/// <typeparam name="TResult">The result type of the execution.</typeparam>
public interface IExecutionContext<TResult>
    where TResult : notnull
{
    /// <summary>
    /// Adds a behavior to this execution context.
    /// </summary>
    /// <typeparam name="TBehavior">The type of behavior to add.</typeparam>
    /// <returns>The execution context with the behavior added.</returns>
    IExecutionContext<TResult> WithBehavior<TBehavior>()
        where TBehavior : class;

    /// <summary>
    /// Adds a behavior instance to this execution context.
    /// </summary>
    /// <param name="behavior">The behavior instance to add.</param>
    /// <returns>The execution context with the behavior added.</returns>
    IExecutionContext<TResult> WithBehavior(object behavior);

    /// <summary>
    /// Executes the use case with the behaviors in this context.
    /// </summary>
    /// <param name="useCaseParameter">The use case parameter to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The execution result.</returns>
    Task<ExecutionResult<TResult>> ExecuteAsync<TUseCaseParameter>(TUseCaseParameter useCaseParameter, CancellationToken cancellationToken = default)
        where TUseCaseParameter : IUseCaseParameter<TResult>;
}

/// <summary>
/// Represents an execution context for untyped use cases.
/// </summary>
public interface IExecutionContext
{
    /// <summary>
    /// Adds a behavior to this execution context.
    /// </summary>
    /// <typeparam name="TBehavior">The type of behavior to add.</typeparam>
    /// <returns>The execution context with the behavior added.</returns>
    IExecutionContext WithBehavior<TBehavior>()
        where TBehavior : class;

    /// <summary>
    /// Adds a behavior instance to this execution context.
    /// </summary>
    /// <param name="behavior">The behavior instance to add.</param>
    /// <returns>The execution context with the behavior added.</returns>
    IExecutionContext WithBehavior(object behavior);

    /// <summary>
    /// Executes the use case with the behaviors in this context.
    /// </summary>
    /// <typeparam name="TResult">The result type of the use case.</typeparam>
    /// <param name="useCaseParameter">The use case parameter to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The execution result.</returns>
    Task<ExecutionResult<TResult>> ExecuteAsync<TResult>(IUseCaseParameter<TResult> useCaseParameter, CancellationToken cancellationToken = default)
        where TResult : notnull;
}