using Microsoft.Extensions.Logging;

namespace FunctionalUseCases.Sample;

/// <summary>
/// Sample implementation of ITransactionManager for demonstration purposes.
/// In a real application, this would integrate with your database system (Entity Framework, ADO.NET, etc.).
/// </summary>
public class SampleTransactionManager : ITransactionManager
{
    private readonly ILogger<SampleTransactionManager> _logger;

    public SampleTransactionManager(ILogger<SampleTransactionManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Beginning sample transaction");
        return Task.FromResult<ITransaction>(new SampleTransaction(_logger));
    }
}

/// <summary>
/// Sample implementation of ITransaction for demonstration purposes.
/// In a real application, this would wrap actual database transactions.
/// </summary>
public class SampleTransaction : ITransaction
{
    private readonly ILogger _logger;
    private bool _disposed = false;
    private bool _committed = false;
    private bool _rolledBack = false;

    public SampleTransaction(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SampleTransaction));
        
        if (_committed || _rolledBack)
            throw new InvalidOperationException("Transaction has already been committed or rolled back");

        _logger.LogDebug("Committing sample transaction");
        _committed = true;
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SampleTransaction));
        
        if (_committed || _rolledBack)
            throw new InvalidOperationException("Transaction has already been committed or rolled back");

        _logger.LogDebug("Rolling back sample transaction");
        _rolledBack = true;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _logger.LogDebug("Disposing sample transaction (Committed: {Committed}, RolledBack: {RolledBack})", 
                _committed, _rolledBack);
            _disposed = true;
        }
    }
}