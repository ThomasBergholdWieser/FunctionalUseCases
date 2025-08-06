namespace FunctionalUseCases;

/// <summary>
/// Execution behavior interface that allows wrapping use case execution with additional logic.
/// Behaviors are executed in the order they are registered, with each behavior able to
/// execute logic before and after the next step in the pipeline.
/// </summary>
/// <typeparam name="TUseCaseParameter">The type of use case parameter being handled.</typeparam>
/// <typeparam name="TResult">The type of result returned by the use case.</typeparam>
public interface IExecutionBehavior<in TUseCaseParameter, TResult>
    where TUseCaseParameter : IUseCaseParameter<TResult>
    where TResult : notnull
{
    /// <summary>
    /// Executes the behavior logic, wrapping the next step in the pipeline.
    /// </summary>
    /// <param name="useCaseParameter">The use case parameter being executed.</param>
    /// <param name="next">The next step in the pipeline (next behavior or the actual use case handler).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An ExecutionResult containing the result or error information.</returns>
    Task<ExecutionResult<TResult>> ExecuteAsync(TUseCaseParameter useCaseParameter, PipelineBehaviorDelegate<TResult> next, CancellationToken cancellationToken = default);
}