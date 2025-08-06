namespace FunctionalUseCases;

/// <summary>
/// Interface for the UseCase dispatcher that handles UseCase execution.
/// </summary>
public interface IUseCaseDispatcher
{
    /// <summary>
    /// Executes a use case.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the use case.</typeparam>
    /// <param name="useCaseParameter">The use case parameter to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An ExecutionResult containing the result or error information.</returns>
    Task<ExecutionResult<TResult>> ExecuteAsync<TResult>(IUseCaseParameter<TResult> useCaseParameter, CancellationToken cancellationToken = default)
        where TResult : notnull;
}