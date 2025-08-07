using Microsoft.Extensions.DependencyInjection;

namespace FunctionalUseCases.Extensions;

/// <summary>
/// Extension methods for creating and working with use case chains.
/// </summary>
public static class UseCaseChainExtensions
{
    /// <summary>
    /// Starts a new use case chain with the specified use case parameter.
    /// </summary>
    /// <typeparam name="TResult">The result type of the first use case.</typeparam>
    /// <param name="dispatcher">The use case dispatcher.</param>
    /// <param name="useCaseParameter">The first use case parameter to execute.</param>
    /// <param name="transactionManager">Optional transaction manager for chain-level transactions.</param>
    /// <param name="logger">Optional logger for transaction logging.</param>
    /// <returns>A new use case chain.</returns>
    public static UseCaseChain<TResult> StartWith<TResult>(this IUseCaseDispatcher dispatcher,
        IUseCaseParameter<TResult> useCaseParameter,
        ITransactionManager? transactionManager = null,
        Microsoft.Extensions.Logging.ILogger? logger = null)
        where TResult : notnull
    {
        if (dispatcher == null)
        {
            throw new ArgumentNullException(nameof(dispatcher));
        }

        if (useCaseParameter == null)
        {
            throw new ArgumentNullException(nameof(useCaseParameter));
        }

        var serviceProvider = GetServiceProvider(dispatcher);
        var chain = new UseCaseChain<TResult>(dispatcher, serviceProvider, transactionManager, logger);
        return chain.Then(useCaseParameter);
    }

    /// <summary>
    /// Starts a new empty use case chain.
    /// Use Then() to add use cases to the chain.
    /// </summary>
    /// <param name="dispatcher">The use case dispatcher.</param>
    /// <param name="transactionManager">Optional transaction manager for chain-level transactions.</param>
    /// <param name="logger">Optional logger for transaction logging.</param>
    /// <returns>A new empty use case chain.</returns>
    public static UseCaseChain StartWith(this IUseCaseDispatcher dispatcher,
        ITransactionManager? transactionManager = null,
        Microsoft.Extensions.Logging.ILogger? logger = null)
    {
        if (dispatcher == null)
        {
            throw new ArgumentNullException(nameof(dispatcher));
        }

        var serviceProvider = GetServiceProvider(dispatcher);
        return new UseCaseChain(dispatcher, serviceProvider, transactionManager, logger);
    }

    private static IServiceProvider GetServiceProvider(IUseCaseDispatcher dispatcher)
    {
        return dispatcher.ServiceProvider;
    }
}