namespace FunctionalUseCases;

/// <summary>
/// Marker interface for use case parameters.
/// All use case parameters should implement this interface to be recognized by the UseCase dispatcher.
/// </summary>
public interface IUseCaseParameter
{
}

/// <summary>
/// Generic interface for use case parameters that define the result type.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the use case.</typeparam>
public interface IUseCaseParameter<out TResult> : IUseCaseParameter
{
}