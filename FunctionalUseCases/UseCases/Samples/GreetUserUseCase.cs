using FunctionalUseCases.UseCases.Core;

namespace FunctionalUseCases.UseCases.Samples;

/// <summary>
/// A simple use case for greeting a user.
/// </summary>
public class GreetUserUseCase : IUseCase<string>
{
    public string Name { get; }

    public GreetUserUseCase(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}