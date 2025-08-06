using FunctionalProcessing;
using Microsoft.Extensions.DependencyInjection;

namespace FunctionalUseCases.UseCases;

/// <summary>
/// Dispatcher that resolves and executes use case handlers via dependency injection.
/// </summary>
public class UseCaseDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the UseCaseDispatcher.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving handlers.</param>
    public UseCaseDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Dispatches a use case to its corresponding handler.
    /// </summary>
    /// <typeparam name="TUseCase">The use case type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="useCase">The use case to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>An ExecutionResult containing the operation result.</returns>
    public async Task<ExecutionResult<TResult>> DispatchAsync<TUseCase, TResult>(
        TUseCase useCase, 
        CancellationToken cancellationToken = default)
        where TUseCase : IUseCase
        where TResult : notnull
    {
        if (useCase == null)
        {
            return new ExecutionResult<TResult>(new ExecutionError("Use case cannot be null"));
        }

        try
        {
            var handler = _serviceProvider.GetRequiredService<IUseCaseHandler<TUseCase, TResult>>();
            return await handler.HandleAsync(useCase, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // Handler not registered
            return new ExecutionResult<TResult>(new ExecutionError($"No handler registered for use case type {typeof(TUseCase).Name}"));
        }
        catch (Exception ex)
        {
            // Other exceptions during handler resolution or execution
            return new ExecutionResult<TResult>(new ExecutionError($"Error executing use case: {ex.Message}"));
        }
    }
}