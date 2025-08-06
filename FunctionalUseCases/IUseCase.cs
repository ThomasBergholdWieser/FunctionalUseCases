namespace FunctionalUseCases;

/// <summary>
/// Marker interface for use cases.
/// All use cases should implement this interface to be recognized by the UseCase dispatcher.
/// </summary>
public interface IUseCase
{
}

/// <summary>
/// Generic interface for use cases that return a result.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the use case.</typeparam>
public interface IUseCase<out TResult> : IUseCase
{
}