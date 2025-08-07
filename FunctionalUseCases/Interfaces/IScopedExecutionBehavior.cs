namespace FunctionalUseCases;

/// <summary>
/// Enhanced execution behavior interface that provides access to execution scope information.
/// This allows behaviors to make decisions based on whether they are executing in a single use case
/// or as part of a use case chain.
/// </summary>
/// <typeparam name="TUseCaseParameter">The type of use case parameter being handled.</typeparam>
/// <typeparam name="TResult">The type of result returned by the use case.</typeparam>
public interface IScopedExecutionBehavior<in TUseCaseParameter, TResult> : IExecutionBehavior<TUseCaseParameter, TResult>
    where TUseCaseParameter : IUseCaseParameter<TResult>
    where TResult : notnull
{
    /// <summary>
    /// Executes the behavior logic with access to execution scope information.
    /// </summary>
    /// <param name="useCaseParameter">The use case parameter being executed.</param>
    /// <param name="scope">The execution scope providing context about the current execution.</param>
    /// <param name="next">The next step in the pipeline (next behavior or the actual use case handler).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An ExecutionResult containing the result or error information.</returns>
    Task<ExecutionResult<TResult>> ExecuteAsync(TUseCaseParameter useCaseParameter, IExecutionScope scope, PipelineBehaviorDelegate<TResult> next, CancellationToken cancellationToken = default);
}

/// <summary>
/// Base class for behaviors that want to be aware of execution scope.
/// Implements the standard IExecutionBehavior interface and provides the enhanced ExecuteAsync method.
/// </summary>
/// <typeparam name="TUseCaseParameter">The type of use case parameter being handled.</typeparam>
/// <typeparam name="TResult">The type of result returned by the use case.</typeparam>
public abstract class ScopedExecutionBehavior<TUseCaseParameter, TResult> : IScopedExecutionBehavior<TUseCaseParameter, TResult>
    where TUseCaseParameter : IUseCaseParameter<TResult>
    where TResult : notnull
{
    /// <summary>
    /// Standard execution method that provides single use case scope by default.
    /// </summary>
    public Task<ExecutionResult<TResult>> ExecuteAsync(TUseCaseParameter useCaseParameter, PipelineBehaviorDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(useCaseParameter, ExecutionScope.SingleUseCase, next, cancellationToken);
    }

    /// <summary>
    /// Enhanced execution method with scope information.
    /// Override this method to implement behavior logic that is aware of execution scope.
    /// </summary>
    public abstract Task<ExecutionResult<TResult>> ExecuteAsync(TUseCaseParameter useCaseParameter, IExecutionScope scope, PipelineBehaviorDelegate<TResult> next, CancellationToken cancellationToken = default);
}