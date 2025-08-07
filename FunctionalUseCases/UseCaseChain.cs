namespace FunctionalUseCases;

/// <summary>
/// Represents a chain of use cases that can be executed sequentially.
/// Execution stops on the first failure, and error handling can be provided at the end of the chain.
/// </summary>
/// <typeparam name="TResult">The final result type of the chain.</typeparam>
public class UseCaseChain<TResult>
    where TResult : notnull
{
    private readonly IUseCaseDispatcher _dispatcher;
    internal readonly List<Func<CancellationToken, Task<ExecutionResult>>> _steps;
    private Func<ExecutionError, CancellationToken, Task<ExecutionResult<TResult>>>? _errorHandler;

    internal UseCaseChain(IUseCaseDispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _steps = new List<Func<CancellationToken, Task<ExecutionResult>>>();
    }

    /// <summary>
    /// Adds a use case to the chain.
    /// </summary>
    /// <typeparam name="TNextResult">The result type of the next use case.</typeparam>
    /// <param name="useCaseParameter">The use case parameter to execute.</param>
    /// <returns>A new chain with the added use case.</returns>
    public UseCaseChain<TNextResult> Then<TNextResult>(IUseCaseParameter<TNextResult> useCaseParameter)
        where TNextResult : notnull
    {
        if (useCaseParameter == null)
        {
            throw new ArgumentNullException(nameof(useCaseParameter));
        }

        var newChain = new UseCaseChain<TNextResult>(_dispatcher);

        // Copy existing steps
        newChain._steps.AddRange(_steps);

        // Add the new step
        newChain._steps.Add(async cancellationToken =>
        {
            var result = await _dispatcher.ExecuteAsync(useCaseParameter, cancellationToken);
            return result; // Implicit conversion from ExecutionResult<T> to ExecutionResult
        });

        return newChain;
    }

    /// <summary>
    /// Specifies an error handler for the chain.
    /// The error handler will be called if any use case in the chain fails.
    /// </summary>
    /// <param name="errorHandler">The error handler function.</param>
    /// <returns>The same chain with error handling configured.</returns>
    public UseCaseChain<TResult> OnError(Func<ExecutionError, Task<ExecutionResult<TResult>>> errorHandler)
    {
        if (errorHandler == null)
        {
            throw new ArgumentNullException(nameof(errorHandler));
        }

        _errorHandler = (error, cancellationToken) => errorHandler(error);
        return this;
    }

    /// <summary>
    /// Specifies an error handler for the chain with cancellation token support.
    /// The error handler will be called if any use case in the chain fails.
    /// </summary>
    /// <param name="errorHandler">The error handler function.</param>
    /// <returns>The same chain with error handling configured.</returns>
    public UseCaseChain<TResult> OnError(Func<ExecutionError, CancellationToken, Task<ExecutionResult<TResult>>> errorHandler)
    {
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        return this;
    }

    /// <summary>
    /// Executes the entire chain of use cases sequentially.
    /// Execution stops on the first failure unless an error handler is provided.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The final execution result.</returns>
    public async Task<ExecutionResult<TResult>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (_steps.Count == 0)
        {
            return Execution.Failure<TResult>("Chain is empty. Add at least one use case using Then().");
        }

        ExecutionResult? lastResult = null;

        try
        {
            // Execute each step in the chain
            foreach (var step in _steps)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var stepResult = await step(cancellationToken);
                lastResult = stepResult;

                // Stop execution on first failure
                if (stepResult.ExecutionFailed)
                {
                    // If we have an error handler, use it
                    if (_errorHandler != null)
                    {
                        return await _errorHandler(stepResult.CheckedError, cancellationToken);
                    }

                    // Otherwise, return the failure result
                    return Execution.Failure<TResult>(stepResult.CheckedError.Messages,
                        stepResult.CheckedError.ErrorCode, stepResult.CheckedError.LogLevel);
                }
            }

            // All steps succeeded, but we need to return TResult
            // The last step should have produced the final result
            if (lastResult is ExecutionResult<TResult> typedResult)
            {
                return typedResult;
            }

            // This shouldn't happen if the chain is constructed correctly
            return Execution.Failure<TResult>("Chain execution completed but final result type mismatch.");
        }
        catch (OperationCanceledException)
        {
            return Execution.Failure<TResult>("Chain execution was cancelled.");
        }
        catch (Exception ex)
        {
            // If we have an error handler, use it for exceptions too
            if (_errorHandler != null)
            {
                var error = new ExecutionError($"Exception during chain execution: {ex.Message}");
                return await _errorHandler(error, cancellationToken);
            }

            return Execution.Failure<TResult>($"Exception during chain execution: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Non-generic version of UseCaseChain for chains that don't return a specific value.
/// </summary>
public class UseCaseChain
{
    private readonly IUseCaseDispatcher _dispatcher;
    private readonly List<Func<CancellationToken, Task<ExecutionResult>>> _steps;
    private Func<ExecutionError, CancellationToken, Task<ExecutionResult>>? _errorHandler;

    internal UseCaseChain(IUseCaseDispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _steps = new List<Func<CancellationToken, Task<ExecutionResult>>>();
    }

    /// <summary>
    /// Adds a use case to the chain.
    /// </summary>
    /// <typeparam name="TResult">The result type of the use case.</typeparam>
    /// <param name="useCaseParameter">The use case parameter to execute.</param>
    /// <returns>A new typed chain with the added use case.</returns>
    public UseCaseChain<TResult> Then<TResult>(IUseCaseParameter<TResult> useCaseParameter)
        where TResult : notnull
    {
        if (useCaseParameter == null)
        {
            throw new ArgumentNullException(nameof(useCaseParameter));
        }

        var newChain = new UseCaseChain<TResult>(_dispatcher);

        // Copy existing steps
        newChain._steps.AddRange(_steps);

        // Add the new step
        newChain._steps.Add(async cancellationToken =>
        {
            var result = await _dispatcher.ExecuteAsync(useCaseParameter, cancellationToken);
            return result; // Implicit conversion from ExecutionResult<T> to ExecutionResult
        });

        return newChain;
    }

    /// <summary>
    /// Specifies an error handler for the chain.
    /// The error handler will be called if any use case in the chain fails.
    /// </summary>
    /// <param name="errorHandler">The error handler function.</param>
    /// <returns>The same chain with error handling configured.</returns>
    public UseCaseChain OnError(Func<ExecutionError, Task<ExecutionResult>> errorHandler)
    {
        if (errorHandler == null)
        {
            throw new ArgumentNullException(nameof(errorHandler));
        }

        _errorHandler = (error, cancellationToken) => errorHandler(error);
        return this;
    }

    /// <summary>
    /// Specifies an error handler for the chain with cancellation token support.
    /// The error handler will be called if any use case in the chain fails.
    /// </summary>
    /// <param name="errorHandler">The error handler function.</param>
    /// <returns>The same chain with error handling configured.</returns>
    public UseCaseChain OnError(Func<ExecutionError, CancellationToken, Task<ExecutionResult>> errorHandler)
    {
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        return this;
    }

    /// <summary>
    /// Executes the entire chain of use cases sequentially.
    /// Execution stops on the first failure unless an error handler is provided.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The final execution result.</returns>
    public async Task<ExecutionResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (_steps.Count == 0)
        {
            return Execution.Failure("Chain is empty. Add at least one use case using Then().");
        }

        try
        {
            // Execute each step in the chain
            foreach (var step in _steps)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var stepResult = await step(cancellationToken);

                // Stop execution on first failure
                if (stepResult.ExecutionFailed)
                {
                    // If we have an error handler, use it
                    if (_errorHandler != null)
                    {
                        return await _errorHandler(stepResult.CheckedError, cancellationToken);
                    }

                    // Otherwise, return the failure result
                    return stepResult;
                }
            }

            // All steps succeeded
            return Execution.Success();
        }
        catch (OperationCanceledException)
        {
            return Execution.Failure("Chain execution was cancelled.");
        }
        catch (Exception ex)
        {
            // If we have an error handler, use it for exceptions too
            if (_errorHandler != null)
            {
                var error = new ExecutionError($"Exception during chain execution: {ex.Message}");
                return await _errorHandler(error, cancellationToken);
            }

            return Execution.Failure($"Exception during chain execution: {ex.Message}", ex);
        }
    }
}