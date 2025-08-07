namespace FunctionalUseCases;

/// <summary>
/// Provides an abstraction for managing transactions within use case execution.
/// This interface allows the TransactionBehavior to work with any transaction system
/// by providing a common contract for transaction lifecycle management.
/// </summary>
public interface ITransactionManager
{
    /// <summary>
    /// Begins a new transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A transaction that can be committed or rolled back.</returns>
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a transaction that can be committed or rolled back.
/// </summary>
public interface ITransaction : IDisposable
{
    /// <summary>
    /// Commits the transaction, making all changes permanent.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the transaction, discarding all changes.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}