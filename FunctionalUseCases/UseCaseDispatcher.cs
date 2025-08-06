using Microsoft.Extensions.DependencyInjection;

namespace FunctionalUseCases;

/// <summary>
/// Interface for the UseCase dispatcher that handles UseCase execution.
/// </summary>
public interface IUseCaseDispatcher
{
    /// <summary>
    /// Dispatches a use case for execution.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the use case.</typeparam>
    /// <param name="useCase">The use case to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An ExecutionResult containing the result or error information.</returns>
    Task<ExecutionResult<TResult>> DispatchAsync<TResult>(IUseCase<TResult> useCase, CancellationToken cancellationToken = default);
}

/// <summary>
/// Mediator-style UseCase dispatcher that resolves handlers via dependency injection.
/// </summary>
public class UseCaseDispatcher : IUseCaseDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="UseCaseDispatcher"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving handlers.</param>
    public UseCaseDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Dispatches a use case for execution by resolving the appropriate handler.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the use case.</typeparam>
    /// <param name="useCase">The use case to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An ExecutionResult containing the result or error information.</returns>
    public async Task<ExecutionResult<TResult>> DispatchAsync<TResult>(IUseCase<TResult> useCase, CancellationToken cancellationToken = default)
    {
        if (useCase == null)
        {
            throw new ArgumentNullException(nameof(useCase));
        }

        try
        {
            var useCaseType = useCase.GetType();
            var handlerType = typeof(IUseCaseHandler<,>).MakeGenericType(useCaseType, typeof(TResult));
            
            var handler = _serviceProvider.GetService(handlerType);
            if (handler == null)
            {
                return ExecutionResult<TResult>.Failure($"No handler registered for use case type '{useCaseType.Name}'");
            }

            var handleMethod = handlerType.GetMethod("HandleAsync");
            if (handleMethod == null)
            {
                return ExecutionResult<TResult>.Failure($"HandleAsync method not found on handler for use case type '{useCaseType.Name}'");
            }


            var handler = _serviceProvider.GetService(handlerType);
            if (handler == null)
            {
                return ExecutionResult<TResult>.Failure($"No handler registered for use case type '{useCaseType.Name}'");
            }

            // Use compiled delegate instead of reflection
            var key = (handlerType, useCaseType, typeof(TResult));
            var handleAsyncDelegate = _handleAsyncDelegateCache.GetOrAdd(key, _ => CreateHandleAsyncDelegate(handlerType, useCaseType, typeof(TResult)));

            var taskObj = handleAsyncDelegate(handler, useCase, cancellationToken);
            var result = await (Task<ExecutionResult<TResult>>)taskObj.ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            return ExecutionResult<TResult>.Failure($"Error dispatching use case: {ex.Message}", ex);
        }
    }
}