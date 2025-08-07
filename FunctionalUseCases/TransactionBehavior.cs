using Microsoft.Extensions.Logging;

namespace FunctionalUseCases;

/// <summary>
/// Execution behavior that wraps use case execution in a transaction.
/// The transaction is committed on successful execution and rolled back on failure or exception.
/// This behavior ensures that all database operations within a use case chain are atomic.
/// </summary>
/// <typeparam name="TUseCaseParameter">The type of use case parameter being handled.</typeparam>
/// <typeparam name="TResult">The type of result returned by the use case.</typeparam>
public class TransactionBehavior<TUseCaseParameter, TResult> : IExecutionBehavior<TUseCaseParameter, TResult>
    where TUseCaseParameter : IUseCaseParameter<TResult>
    where TResult : notnull
{
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<TransactionBehavior<TUseCaseParameter, TResult>> _logger;

    public TransactionBehavior(
        ITransactionManager transactionManager,
        ILogger<TransactionBehavior<TUseCaseParameter, TResult>> logger)
    {
        _transactionManager = transactionManager ?? throw new ArgumentNullException(nameof(transactionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ExecutionResult<TResult>> ExecuteAsync(TUseCaseParameter useCaseParameter, PipelineBehaviorDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        var useCaseParameterName = typeof(TUseCaseParameter).Name;

        _logger.LogDebug("Starting transaction for use case: {UseCaseParameterName}", useCaseParameterName);

        ITransaction? transaction = null;
        try
        {
            // Begin transaction
            transaction = await _transactionManager.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogDebug("Transaction started for use case: {UseCaseParameterName}", useCaseParameterName);

            // Execute the next step in the pipeline
            var result = await next().ConfigureAwait(false);

            if (result.ExecutionSucceeded)
            {
                // Commit transaction on success
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogDebug("Transaction committed for use case: {UseCaseParameterName}", useCaseParameterName);
            }
            else
            {
                // Rollback transaction on failure
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogDebug("Transaction rolled back for use case: {UseCaseParameterName} due to execution failure", useCaseParameterName);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during transaction execution for use case: {UseCaseParameterName}", useCaseParameterName);

            // Rollback transaction on exception
            if (transaction != null)
            {
                try
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    _logger.LogDebug("Transaction rolled back for use case: {UseCaseParameterName} due to exception", useCaseParameterName);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Failed to rollback transaction for use case: {UseCaseParameterName}", useCaseParameterName);
                    // Don't throw rollback exception, preserve original exception
                }
            }

            return Execution.Failure<TResult>($"Exception in TransactionBehavior: {ex.Message}", ex);
        }
        finally
        {
            // Ensure transaction is disposed
            transaction?.Dispose();
        }
    }
}