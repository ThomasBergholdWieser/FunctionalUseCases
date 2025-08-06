using FunctionalProcessing;

namespace FunctionalUseCases.UseCases.Core;

/// <summary>
/// Defines a handler for a use case that doesn't return a value.
/// </summary>
/// <typeparam name="TUseCase">The type of use case to handle.</typeparam>
public interface IUseCaseHandler<in TUseCase>
    where TUseCase : class, IUseCase
{
    /// <summary>
    /// Handles the specified use case.
    /// </summary>
    /// <param name="useCase">The use case to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An ExecutionResult indicating the outcome of the operation.</returns>
    Task<ExecutionResult> Handle(TUseCase useCase, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a handler for a use case that returns a value.
/// </summary>
/// <typeparam name="TUseCase">The type of use case to handle.</typeparam>
/// <typeparam name="TResult">The type of value returned by the use case.</typeparam>
public interface IUseCaseHandler<in TUseCase, TResult>
    where TUseCase : class, IUseCase<TResult>
    where TResult : notnull
{
    /// <summary>
    /// Handles the specified use case.
    /// </summary>
    /// <param name="useCase">The use case to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An ExecutionResult containing the result value or error.</returns>
    Task<ExecutionResult<TResult>> Handle(TUseCase useCase, CancellationToken cancellationToken = default);
}