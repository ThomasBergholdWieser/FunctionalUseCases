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
    /// <returns>A new use case chain.</returns>
    public static UseCaseChain<TResult> StartWith<TResult>(this IUseCaseDispatcher dispatcher, IUseCaseParameter<TResult> useCaseParameter)
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

        var chain = new UseCaseChain<TResult>(dispatcher);
        return chain.Then(useCaseParameter);
    }

    /// <summary>
    /// Starts a new empty use case chain.
    /// Use Then() to add use cases to the chain.
    /// </summary>
    /// <param name="dispatcher">The use case dispatcher.</param>
    /// <returns>A new empty use case chain.</returns>
    public static UseCaseChain StartWith(this IUseCaseDispatcher dispatcher)
    {
        if (dispatcher == null)
        {
            throw new ArgumentNullException(nameof(dispatcher));
        }

        return new UseCaseChain(dispatcher);
    }
}