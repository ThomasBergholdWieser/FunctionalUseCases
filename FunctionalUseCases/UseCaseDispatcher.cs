using Microsoft.Extensions.DependencyInjection;

namespace FunctionalUseCases;

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
    /// Executes a use case by resolving the appropriate handler and running it through the pipeline behaviors.
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

            // Get all pipeline behaviors for this use case parameter and result type
            var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(useCaseParameterType, typeof(TResult));
            var behaviors = _serviceProvider.GetServices(behaviorType).ToArray();

            // Build the pipeline by chaining behaviors
            PipelineBehaviorDelegate<TResult> pipeline = async () =>
            {
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

                return await task.ConfigureAwait(false);
            };

            // Wrap the pipeline with behaviors in reverse order (so they execute in registration order)
            for (int i = behaviors.Length - 1; i >= 0; i--)
            {
                var behavior = behaviors[i];
                var currentPipeline = pipeline;

                // Create a new pipeline that wraps the current one with this behavior
                pipeline = () =>
                {
                    var handleMethod = behaviorType.GetMethod("HandleAsync");
                    if (handleMethod == null)
                    {
                        return currentPipeline();
                    }

                    var task = (Task<ExecutionResult<TResult>>?)handleMethod.Invoke(behavior, new object[] { useCaseParameter, currentPipeline, cancellationToken });
                    return task ?? currentPipeline();
                };
            }

            // Execute the complete pipeline
            var result = await pipeline().ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            return Execution.Failure<TResult>($"Error executing use case: {ex.Message}", ex);
        }
    }
}