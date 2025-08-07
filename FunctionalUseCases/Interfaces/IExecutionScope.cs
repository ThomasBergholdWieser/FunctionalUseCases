namespace FunctionalUseCases;

/// <summary>
/// Provides context information about the current execution scope.
/// This allows behaviors to determine if they are executing in a single use case
/// or as part of a use case chain.
/// </summary>
public interface IExecutionScope
{
    /// <summary>
    /// Gets a value indicating whether the current execution is part of a use case chain.
    /// </summary>
    bool IsChainExecution { get; }

    /// <summary>
    /// Gets a value indicating whether this is the first use case in a chain.
    /// Only meaningful when IsChainExecution is true.
    /// </summary>
    bool IsChainStart { get; }

    /// <summary>
    /// Gets a value indicating whether this is the last use case in a chain.
    /// Only meaningful when IsChainExecution is true.
    /// </summary>
    bool IsChainEnd { get; }

    /// <summary>
    /// Gets the chain identifier if this execution is part of a chain.
    /// </summary>
    string? ChainId { get; }
}

/// <summary>
/// Implementation of execution scope.
/// </summary>
public class ExecutionScope : IExecutionScope
{
    public bool IsChainExecution { get; init; }
    public bool IsChainStart { get; init; }
    public bool IsChainEnd { get; init; }
    public string? ChainId { get; init; }

    /// <summary>
    /// Creates an execution scope for a single use case execution.
    /// </summary>
    public static ExecutionScope SingleUseCase => new()
    {
        IsChainExecution = false,
        IsChainStart = false,
        IsChainEnd = false,
        ChainId = null
    };

    /// <summary>
    /// Creates an execution scope for a use case chain.
    /// </summary>
    /// <param name="chainId">The chain identifier.</param>
    /// <param name="isStart">Whether this is the start of the chain.</param>
    /// <param name="isEnd">Whether this is the end of the chain.</param>
    public static ExecutionScope Chain(string chainId, bool isStart, bool isEnd) => new()
    {
        IsChainExecution = true,
        IsChainStart = isStart,
        IsChainEnd = isEnd,
        ChainId = chainId
    };
}