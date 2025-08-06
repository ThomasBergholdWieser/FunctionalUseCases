using Microsoft.Extensions.DependencyInjection;

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
    /// Executes a use case by resolving the appropriate handler.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the use case.</typeparam>
    /// <param name="useCaseParameter">The use case parameter to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An ExecutionResult containing the result or error information.</returns>
    public async Task<ExecutionResult<TResult>> ExecuteAsync<TResult>(IUseCaseParameter<TResult> useCaseParameter, CancellationToken cancellationToken = default)
        where TResult : notnull
    {
        if (useCaseParameter == null)
        {
            throw new ArgumentNullException(nameof(useCaseParameter));
        }

        try
        {
            var useCaseParameterType = useCaseParameter.GetType();
            var useCaseType = typeof(IUseCase<,>).MakeGenericType(useCaseParameterType, typeof(TResult));
            
            var useCase = _serviceProvider.GetService(useCaseType);
            if (useCase == null)
            {
                return Execution.Failure<TResult>($"No use case registered for parameter type '{useCaseParameterType.Name}'");
            }

            var executeMethod = useCaseType.GetMethod("ExecuteAsync");
            if (executeMethod == null)
            {
                return Execution.Failure<TResult>($"ExecuteAsync method not found on use case for parameter type '{useCaseParameterType.Name}'");
            }

            // Use reflection to call ExecuteAsync
            var task = (Task<ExecutionResult<TResult>>?)executeMethod.Invoke(useCase, new object[] { useCaseParameter, cancellationToken });
            if (task == null)
            {
                return Execution.Failure<TResult>($"ExecuteAsync method returned null for parameter type '{useCaseParameterType.Name}'");
            }

            var result = await task.ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            return Execution.Failure<TResult>($"Error executing use case: {ex.Message}", ex);
        }
    }
}