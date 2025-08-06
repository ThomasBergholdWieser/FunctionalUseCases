using FunctionalProcessing;

namespace FunctionalUseCases.UseCases.Core;

/// <summary>
/// Marker interface for use cases that don't return a value.
/// </summary>
public interface IUseCase
{
}

/// <summary>
/// Marker interface for use cases that return a value of type T.
/// </summary>
/// <typeparam name="T">The type of value returned by the use case.</typeparam>
public interface IUseCase<out T> : IUseCase
{
}