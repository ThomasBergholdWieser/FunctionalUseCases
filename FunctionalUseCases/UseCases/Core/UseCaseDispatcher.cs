using FunctionalProcessing;
using Microsoft.Extensions.DependencyInjection;

namespace FunctionalUseCases.UseCases.Core;

/// <summary>
/// Defines the interface for dispatching use cases to their respective handlers.
/// </summary>
public interface IUseCaseDispatcher
{
    /// <summary>
    /// Dispatches a use case that doesn't return a value to its handler.
    /// </summary>
    /// <typeparam name="TUseCase">The type of use case to dispatch.</typeparam>
    /// <param name="useCase">The use case to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An ExecutionResult indicating the outcome of the operation.</returns>
    Task<ExecutionResult> Dispatch<TUseCase>(TUseCase useCase, CancellationToken cancellationToken = default)
        where TUseCase : class, IUseCase;

    /// <summary>
    /// Dispatches a use case that returns a value to its handler.
    /// </summary>
    /// <typeparam name="TUseCase">The type of use case to dispatch.</typeparam>
    /// <typeparam name="TResult">The type of value returned by the use case.</typeparam>
    /// <param name="useCase">The use case to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An ExecutionResult containing the result value or error.</returns>
    Task<ExecutionResult<TResult>> Dispatch<TUseCase, TResult>(TUseCase useCase, CancellationToken cancellationToken = default)
        where TUseCase : class, IUseCase<TResult>
        where TResult : notnull;
}

/// <summary>
/// Default implementation of the use case dispatcher that resolves handlers via dependency injection.
/// </summary>
public class UseCaseDispatcher : IUseCaseDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public UseCaseDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public async Task<ExecutionResult> Dispatch<TUseCase>(TUseCase useCase, CancellationToken cancellationToken = default)
        where TUseCase : class, IUseCase
    {
        ArgumentNullException.ThrowIfNull(useCase);

        var handler = _serviceProvider.GetService<IUseCaseHandler<TUseCase>>();
        if (handler == null)
        {
            return new ExecutionResult(new ExecutionError($"No handler registered for use case type {typeof(TUseCase).Name}"));
        }

        try
        {
            return await handler.Handle(useCase, cancellationToken);
        }
        catch (Exception ex)
        {
            return new ExecutionResult(new ExecutionError($"Handler execution failed: {ex.Message}"));
        }
    }

    /// <inheritdoc />
    public async Task<ExecutionResult<TResult>> Dispatch<TUseCase, TResult>(TUseCase useCase, CancellationToken cancellationToken = default)
        where TUseCase : class, IUseCase<TResult>
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(useCase);

        var handler = _serviceProvider.GetService<IUseCaseHandler<TUseCase, TResult>>();
        if (handler == null)
        {
            return new ExecutionResult<TResult>(new ExecutionError($"No handler registered for use case type {typeof(TUseCase).Name}"));
        }

        try
        {
            return await handler.Handle(useCase, cancellationToken);
        }
        catch (Exception ex)
        {
            return new ExecutionResult<TResult>(new ExecutionError($"Handler execution failed: {ex.Message}"));
        }
    }
}