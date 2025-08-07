using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FunctionalUseCases;

/// <summary>
/// Execution behavior that wraps use case execution in a transaction.
/// This behavior is chain-aware:
/// - For single use cases: Creates, commits/rollbacks transaction per use case
/// - For use case chains: Creates transaction at chain start, commits/rollbacks at chain end
/// The behavior ensures that all database operations within a use case or chain are atomic.
/// </summary>
/// <typeparam name="TUseCaseParameter">The type of use case parameter being handled.</typeparam>
/// <typeparam name="TResult">The type of result returned by the use case.</typeparam>
public class TransactionBehavior<TUseCaseParameter, TResult> : ScopedExecutionBehavior<TUseCaseParameter, TResult>
    where TUseCaseParameter : IUseCaseParameter<TResult>
    where TResult : notnull
{
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<TransactionBehavior<TUseCaseParameter, TResult>> _logger;
    
    // Static storage for chain transactions (shared across all instances)
    private static readonly ConcurrentDictionary<string, ITransaction> _chainTransactions = new();

    public TransactionBehavior(
        ITransactionManager transactionManager,
        ILogger<TransactionBehavior<TUseCaseParameter, TResult>> logger)
    {
        _transactionManager = transactionManager ?? throw new ArgumentNullException(nameof(transactionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<ExecutionResult<TResult>> ExecuteAsync(TUseCaseParameter useCaseParameter, IExecutionScope scope, PipelineBehaviorDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        var useCaseParameterName = typeof(TUseCaseParameter).Name;

        if (scope.IsChainExecution)
        {
            return await ExecuteInChainAsync(useCaseParameter, scope, next, cancellationToken, useCaseParameterName);
        }
        else
        {
            return await ExecuteInSingleUseCaseAsync(useCaseParameter, next, cancellationToken, useCaseParameterName);
        }
    }

    private async Task<ExecutionResult<TResult>> ExecuteInSingleUseCaseAsync(TUseCaseParameter useCaseParameter, PipelineBehaviorDelegate<TResult> next, CancellationToken cancellationToken, string useCaseParameterName)
    {
        _logger.LogDebug("Starting transaction for single use case: {UseCaseParameterName}", useCaseParameterName);

        ITransaction? transaction = null;
        try
        {
            // Begin transaction
            transaction = await _transactionManager.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogDebug("Transaction started for single use case: {UseCaseParameterName}", useCaseParameterName);

            // Execute the next step in the pipeline
            var result = await next().ConfigureAwait(false);

            if (result.ExecutionSucceeded)
            {
                // Commit transaction on success
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogDebug("Transaction committed for single use case: {UseCaseParameterName}", useCaseParameterName);
            }
            else
            {
                // Rollback transaction on failure
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogDebug("Transaction rolled back for single use case: {UseCaseParameterName} due to execution failure", useCaseParameterName);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during transaction execution for single use case: {UseCaseParameterName}", useCaseParameterName);

            // Rollback transaction on exception
            if (transaction != null)
            {
                try
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    _logger.LogDebug("Transaction rolled back for single use case: {UseCaseParameterName} due to exception", useCaseParameterName);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Failed to rollback transaction for single use case: {UseCaseParameterName}", useCaseParameterName);
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

    private async Task<ExecutionResult<TResult>> ExecuteInChainAsync(TUseCaseParameter useCaseParameter, IExecutionScope scope, PipelineBehaviorDelegate<TResult> next, CancellationToken cancellationToken, string useCaseParameterName)
    {
        var chainId = scope.ChainId!;
        
        if (scope.IsChainStart)
        {
            // Start transaction for the entire chain
            _logger.LogDebug("Starting transaction for use case chain: {ChainId}, use case: {UseCaseParameterName}", chainId, useCaseParameterName);
            
            try
            {
                var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                _chainTransactions[chainId] = transaction;
                _logger.LogDebug("Transaction started for use case chain: {ChainId}", chainId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start transaction for use case chain: {ChainId}", chainId);
                return Execution.Failure<TResult>($"Failed to start transaction for chain: {ex.Message}", ex);
            }
        }

        try
        {
            // Execute the next step in the pipeline
            var result = await next().ConfigureAwait(false);

            if (scope.IsChainEnd)
            {
                // Commit or rollback transaction at the end of the chain
                if (_chainTransactions.TryRemove(chainId, out var transaction))
                {
                    try
                    {
                        if (result.ExecutionSucceeded)
                        {
                            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                            _logger.LogDebug("Transaction committed for use case chain: {ChainId}", chainId);
                        }
                        else
                        {
                            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                            _logger.LogDebug("Transaction rolled back for use case chain: {ChainId} due to execution failure", chainId);
                        }
                    }
                    finally
                    {
                        transaction.Dispose();
                    }
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during transaction execution for use case chain: {ChainId}, use case: {UseCaseParameterName}", chainId, useCaseParameterName);

            if (scope.IsChainEnd || !_chainTransactions.ContainsKey(chainId))
            {
                // Rollback transaction on exception at chain end or if transaction is missing
                if (_chainTransactions.TryRemove(chainId, out var transaction))
                {
                    try
                    {
                        await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                        _logger.LogDebug("Transaction rolled back for use case chain: {ChainId} due to exception", chainId);
                    }
                    catch (Exception rollbackEx)
                    {
                        _logger.LogError(rollbackEx, "Failed to rollback transaction for use case chain: {ChainId}", chainId);
                    }
                    finally
                    {
                        transaction.Dispose();
                    }
                }
            }

            return Execution.Failure<TResult>($"Exception in TransactionBehavior: {ex.Message}", ex);
        }
    }
}