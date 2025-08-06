namespace FunctionalUseCases;

/// <summary>
/// Delegate type representing the next step in the pipeline behavior chain.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the use case.</typeparam>
/// <returns>An ExecutionResult containing the result or error information.</returns>
public delegate Task<ExecutionResult<TResult>> PipelineBehaviorDelegate<TResult>()
    where TResult : notnull;