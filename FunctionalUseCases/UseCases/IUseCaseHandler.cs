using FunctionalProcessing;

namespace FunctionalUseCases.UseCases;

/// <summary>
/// Generic handler interface for use cases.
/// </summary>
/// <typeparam name="TUseCase">The use case type that implements IUseCase.</typeparam>
/// <typeparam name="TResult">The result type returned by the handler.</typeparam>
public interface IUseCaseHandler<in TUseCase, TResult>
    where TUseCase : IUseCase
    where TResult : notnull
{
    /// <summary>
    /// Handles the specified use case.
    /// </summary>
    /// <param name="useCase">The use case to handle.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>An ExecutionResult containing the operation result.</returns>
    Task<ExecutionResult<TResult>> HandleAsync(TUseCase useCase, CancellationToken cancellationToken = default);
}