using Microsoft.Extensions.DependencyInjection;

namespace FunctionalUseCases;

/// <summary>
/// Implementation of execution context that manages per-call behaviors.
/// </summary>
/// <typeparam name="TResult">The result type of the execution.</typeparam>
internal class ExecutionContext<TResult> : IExecutionContext<TResult>
    where TResult : notnull
{
    private readonly IUseCaseDispatcher _dispatcher;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<object> _perCallBehaviors;

    public ExecutionContext(IUseCaseDispatcher dispatcher, IServiceProvider serviceProvider)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _perCallBehaviors = new List<object>();
    }

    internal ExecutionContext(IUseCaseDispatcher dispatcher, IServiceProvider serviceProvider, List<object> existingBehaviors)
    {
        _dispatcher = dispatcher;
        _serviceProvider = serviceProvider;
        _perCallBehaviors = new List<object>(existingBehaviors);
    }

    public IExecutionContext<TResult> WithBehavior<TBehavior>()
        where TBehavior : class
    {
        var behavior = _serviceProvider.GetRequiredService<TBehavior>();
        return WithBehavior(behavior);
    }

    public IExecutionContext<TResult> WithBehavior(object behavior)
    {
        if (behavior == null)
        {
            throw new ArgumentNullException(nameof(behavior));
        }

        var newBehaviors = new List<object>(_perCallBehaviors) { behavior };
        return new ExecutionContext<TResult>(_dispatcher, _serviceProvider, newBehaviors);
    }

    public async Task<ExecutionResult<TResult>> ExecuteAsync<TUseCaseParameter>(TUseCaseParameter useCaseParameter, CancellationToken cancellationToken = default)
        where TUseCaseParameter : IUseCaseParameter<TResult>
    {
        return await ExecuteInternalAsync(useCaseParameter, ExecutionScope.SingleUseCase, cancellationToken);
    }

    internal async Task<ExecutionResult<TResult>> ExecuteInternalAsync<TUseCaseParameter>(TUseCaseParameter useCaseParameter, IExecutionScope scope, CancellationToken cancellationToken = default)
        where TUseCaseParameter : IUseCaseParameter<TResult>
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

            // Get global execution behaviors
            var behaviorType = typeof(IExecutionBehavior<,>).MakeGenericType(useCaseParameterType, typeof(TResult));
            var globalBehaviors = _serviceProvider.GetServices(behaviorType).ToArray();

            // Filter per-call behaviors for this use case parameter type
            var applicablePerCallBehaviors = _perCallBehaviors
                .Where(b => behaviorType.IsInstanceOfType(b))
                .ToArray();

            // Combine global and per-call behaviors (per-call behaviors run first)
            var allBehaviors = applicablePerCallBehaviors.Concat(globalBehaviors).ToArray();

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
            for (int i = allBehaviors.Length - 1; i >= 0; i--)
            {
                var behavior = allBehaviors[i];
                var currentPipeline = pipeline;

                // Create a new pipeline that wraps the current one with this behavior
                pipeline = () =>
                {
                    // Check if this is a scoped behavior that needs execution scope
                    var scopedBehaviorType = typeof(IScopedExecutionBehavior<,>).MakeGenericType(useCaseParameterType, typeof(TResult));
                    if (scopedBehaviorType.IsInstanceOfType(behavior))
                    {
                        var executeMethod = scopedBehaviorType.GetMethod("ExecuteAsync", 
                            new[] { useCaseParameterType, typeof(IExecutionScope), typeof(PipelineBehaviorDelegate<TResult>), typeof(CancellationToken) });
                        if (executeMethod != null)
                        {
                            var task = (Task<ExecutionResult<TResult>>?)executeMethod.Invoke(behavior, new object[] { useCaseParameter, scope, currentPipeline, cancellationToken });
                            return task ?? currentPipeline();
                        }
                    }

                    // Fall back to standard behavior execution
                    var executeStandardMethod = behaviorType.GetMethod("ExecuteAsync");
                    if (executeStandardMethod == null)
                    {
                        return currentPipeline();
                    }

                    var standardTask = (Task<ExecutionResult<TResult>>?)executeStandardMethod.Invoke(behavior, new object[] { useCaseParameter, currentPipeline, cancellationToken });
                    return standardTask ?? currentPipeline();
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

/// <summary>
/// Implementation of untyped execution context.
/// </summary>
internal class ExecutionContext : IExecutionContext
{
    private readonly IUseCaseDispatcher _dispatcher;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<object> _perCallBehaviors;

    public ExecutionContext(IUseCaseDispatcher dispatcher, IServiceProvider serviceProvider)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _perCallBehaviors = new List<object>();
    }

    internal ExecutionContext(IUseCaseDispatcher dispatcher, IServiceProvider serviceProvider, List<object> existingBehaviors)
    {
        _dispatcher = dispatcher;
        _serviceProvider = serviceProvider;
        _perCallBehaviors = new List<object>(existingBehaviors);
    }

    public IExecutionContext WithBehavior<TBehavior>()
        where TBehavior : class
    {
        var behavior = _serviceProvider.GetRequiredService<TBehavior>();
        return WithBehavior(behavior);
    }

    public IExecutionContext WithBehavior(object behavior)
    {
        if (behavior == null)
        {
            throw new ArgumentNullException(nameof(behavior));
        }

        var newBehaviors = new List<object>(_perCallBehaviors) { behavior };
        return new ExecutionContext(_dispatcher, _serviceProvider, newBehaviors);
    }

    public async Task<ExecutionResult<TResult>> ExecuteAsync<TResult>(IUseCaseParameter<TResult> useCaseParameter, CancellationToken cancellationToken = default)
        where TResult : notnull
    {
        var typedContext = new ExecutionContext<TResult>(_dispatcher, _serviceProvider, _perCallBehaviors);
        return await typedContext.ExecuteAsync(useCaseParameter, cancellationToken);
    }
}