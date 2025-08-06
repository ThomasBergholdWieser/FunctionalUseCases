namespace FunctionalUseCases;

/// <summary>
/// Generic handler interface for use cases that return ExecutionResult.
/// </summary>
/// <typeparam name="TUseCase">The type of use case to handle.</typeparam>
/// <typeparam name="TResult">The type of result returned by the use case.</typeparam>
public interface IUseCaseHandler<in TUseCase, TResult>
    where TUseCase : IUseCase<TResult>
{
    /// <summary>
    /// Handles the execution of the use case.
    /// </summary>
    /// <param name="useCase">The use case to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An ExecutionResult containing the result or error information.</returns>
    Task<ExecutionResult<TResult>> HandleAsync(TUseCase useCase, CancellationToken cancellationToken = default);
}