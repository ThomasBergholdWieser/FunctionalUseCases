namespace FunctionalUseCases;

/// <summary>
/// Generic use case interface that handles execution and returns ExecutionResult.
/// </summary>
/// <typeparam name="TUseCaseParameter">The type of use case parameter to handle.</typeparam>
/// <typeparam name="TResult">The type of result returned by the use case.</typeparam>
public interface IUseCase<in TUseCaseParameter, TResult>
    where TUseCaseParameter : IUseCaseParameter<TResult>
    where TResult : notnull
{
    /// <summary>
    /// Executes the use case.
    /// </summary>
    /// <param name="useCaseParameter">The use case parameter to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An ExecutionResult containing the result or error information.</returns>
    Task<ExecutionResult<TResult>> ExecuteAsync(TUseCaseParameter useCaseParameter, CancellationToken cancellationToken = default);
}